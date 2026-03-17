using ImageMinify.Models;
using ImageMinify.Services;
using ImageMinify.ViewModels;

namespace ImageMinify.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task InitializeAsync_RestoresPersistedSelectionsAndSettings()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-vm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var filePath = Path.Combine(tempDirectory, "sample.jpg");
            File.WriteAllText(filePath, "data");

            var store = new InMemorySettingsStore();
            var settingsService = new SettingsService(store);
            await settingsService.SaveAsync(new AppSettingsSnapshot
            {
                Quality = 64,
                OutputFormat = "png",
                RenameEnabled = true,
                RenamePrefix = "batch",
                RenameSeparator = "_",
                RenameStartNumber = 10,
                LastFiles = [filePath],
                LastMode = "files",
            });

            var viewModel = new MainViewModel(
                new FakeImageCompressor(),
                new FileRenameService(),
                settingsService);

            await viewModel.InitializeAsync();

            Assert.Equal(64, viewModel.Quality);
            Assert.Equal("png", viewModel.OutputFormat);
            Assert.True(viewModel.IsRenameEnabled);
            Assert.Equal("batch", viewModel.RenamePrefix);
            Assert.Equal("_", viewModel.RenameSeparator);
            Assert.Equal(10, viewModel.StartNumber);
            Assert.Equal("sample.jpg", viewModel.SelectedFilesDisplay);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task StartCompressionAsync_CompressesSelectedFilesAndBuildsSummary()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-vm-run-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var fileA = Path.Combine(tempDirectory, "a.jpg");
            var fileB = Path.Combine(tempDirectory, "b.png");
            File.WriteAllText(fileA, "a");
            File.WriteAllText(fileB, "b");

            var viewModel = new MainViewModel(
                new FakeImageCompressor(),
                new FileRenameService(),
                new SettingsService(new InMemorySettingsStore()))
            {
                SelectFilesHandler = () => Task.FromResult<IReadOnlyList<string>>([fileA, fileB]),
            };

            await viewModel.InitializeAsync();
            await viewModel.SelectFilesCommand.ExecuteAsync(null);
            await viewModel.StartCompressionCommand.ExecuteAsync(null);

            Assert.Equal(2, viewModel.Results.Count);
            Assert.Equal(2, viewModel.Summary.TotalFiles);
            Assert.Equal(2, viewModel.Summary.ProcessedFiles);
            Assert.True(viewModel.IsStatusOpen);
            Assert.Equal(100, viewModel.ProgressValue);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private sealed class FakeImageCompressor : IImageCompressor
    {
        public IReadOnlyList<string> SupportedExtensions { get; } = [".jpg", ".jpeg", ".png", ".bmp", ".webp"];

        public Task<CompressionResult> CompressAsync(string inputPath, string? outputPath = null, string outputFormat = "original", int quality = 85, CancellationToken ct = default)
        {
            var extension = outputFormat switch
            {
                "jpeg" => ".jpg",
                "png" => ".png",
                "webp" => ".webp",
                _ => Path.GetExtension(inputPath),
            };

            var targetPath = outputPath ?? Path.Combine(
                Path.GetDirectoryName(inputPath) ?? AppContext.BaseDirectory,
                $"{Path.GetFileNameWithoutExtension(inputPath)}_compressed{extension}");

            File.WriteAllText(targetPath, "compressed");

            return Task.FromResult(new CompressionResult
            {
                OriginalPath = inputPath,
                CompressedPath = targetPath,
                OriginalSize = 100,
                CompressedSize = 60,
                OriginalSizeFormatted = "100.00 B",
                CompressedSizeFormatted = "60.00 B",
                CompressionRatio = 40,
                Success = true,
            });
        }

        public Task<(List<CompressionResult> Results, CompressionSummary Summary)> CompressDirectoryAsync(string directoryPath, string outputFormat = "original", int quality = 85, IProgress<(int Processed, int Total, CompressionResult? Result)>? progress = null, CancellationToken ct = default)
        {
            var files = Directory.EnumerateFiles(directoryPath)
                .Where(path => SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .ToList();

            var results = new List<CompressionResult>();

            for (var index = 0; index < files.Count; index++)
            {
                var result = CompressAsync(files[index], outputFormat: outputFormat, quality: quality, ct: ct).Result;
                results.Add(result);
                progress?.Report((index + 1, files.Count, result));
            }

            return Task.FromResult((results, CompressionSummary.FromResults(files.Count, files.Count, results)));
        }

        public EngineCapabilities GetCapabilities()
        {
            return new EngineCapabilities
            {
                HasMozJpeg = true,
                HasImagequant = true,
                HasOxipng = true,
                JpegOptimizer = "MozJPEG",
                PngQuantization = "imagequant",
                PngLossless = "oxipng",
            };
        }
    }
}

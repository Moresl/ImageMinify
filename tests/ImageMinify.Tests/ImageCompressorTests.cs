using ImageMinify.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageMinify.Tests;

public sealed class ImageCompressorTests : IDisposable
{
    private readonly IImageCompressor _compressor = new ImageCompressor(
        new JpegCompressor(new ExifService()),
        new PngCompressor(new ExifService()),
        new WebpCompressor(new ExifService()));
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-compressor-{Guid.NewGuid():N}");

    public ImageCompressorTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Theory]
    [InlineData("example.jpg")]
    [InlineData("example.jpeg")]
    [InlineData("example.png")]
    [InlineData("example.webp")]
    [InlineData("example.bmp")]
    public void SupportedExtensions_ContainExpectedImageTypes(string fileName)
    {
        Assert.Contains(
            _compressor.SupportedExtensions,
            extension => string.Equals(extension, Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CompressAsync_ConvertsToRequestedFormat()
    {
        var inputPath = Path.Combine(_tempDirectory, "source.jpg");
        using (var image = new Image<Rgba32>(80, 80, Color.Orange))
        {
            image.Save(inputPath);
        }

        var result = await _compressor.CompressAsync(inputPath, outputFormat: "webp", quality: 80);

        Assert.True(result.Success);
        Assert.True(File.Exists(result.CompressedPath));
        Assert.EndsWith(".webp", result.CompressedPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.OriginalSize > 0);
        Assert.True(result.CompressedSize > 0);
    }

    [Fact]
    public async Task CompressDirectoryAsync_ProcessesSupportedFilesOnly()
    {
        using (var jpg = new Image<Rgba32>(48, 48, Color.Blue))
        {
            jpg.Save(Path.Combine(_tempDirectory, "one.jpg"));
        }

        using (var png = new Image<Rgba32>(48, 48, Color.Green))
        {
            png.Save(Path.Combine(_tempDirectory, "two.png"));
        }

        File.WriteAllText(Path.Combine(_tempDirectory, "skip.txt"), "not an image");

        var (_, summary) = await _compressor.CompressDirectoryAsync(_tempDirectory, outputFormat: "jpeg", quality: 75);

        Assert.Equal(2, summary.TotalFiles);
        Assert.Equal(2, summary.ProcessedFiles);
        Assert.True(summary.TotalOriginalSize > 0);
        Assert.True(summary.TotalCompressedSize > 0);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}

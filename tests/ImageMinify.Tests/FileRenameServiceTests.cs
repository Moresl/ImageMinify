using ImageMinify.Models;
using ImageMinify.Services;

namespace ImageMinify.Tests;

public sealed class FileRenameServiceTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-tests-{Guid.NewGuid():N}");

    public FileRenameServiceTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void BuildFileName_UsesConfiguredPrefixSeparatorAndDigits()
    {
        var service = new FileRenameService();
        var settings = new RenameSettings
        {
            Enabled = true,
            Prefix = "img",
            Separator = "-",
            StartNumber = 1,
        };

        var fileName = service.BuildFileName(settings, processed: 3, total: 15, extension: ".jpg");
        Assert.Equal("img-03.jpg", fileName);
    }

    [Fact]
    public void ApplyRename_RenamesExistingOutputFile()
    {
        var sourcePath = Path.Combine(_tempDirectory, "example_compressed.jpg");
        File.WriteAllText(sourcePath, "data");

        var result = new CompressionResult
        {
            OriginalPath = Path.Combine(_tempDirectory, "example.jpg"),
            CompressedPath = sourcePath,
            Success = true,
        };

        var renamed = new FileRenameService().ApplyRename(
            result,
            new RenameSettings
            {
                Enabled = true,
                Prefix = "photo",
                Separator = "_",
                StartNumber = 1,
            },
            processed: 1,
            total: 9);

        Assert.NotEqual(sourcePath, renamed.CompressedPath);
        Assert.True(File.Exists(renamed.CompressedPath));
        Assert.False(File.Exists(sourcePath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}

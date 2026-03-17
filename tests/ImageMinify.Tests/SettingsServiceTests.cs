using ImageMinify.Models;
using ImageMinify.Services;

namespace ImageMinify.Tests;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-settings-{Guid.NewGuid():N}");

    public SettingsServiceTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsSnapshot()
    {
        var fileA = Path.Combine(_tempDirectory, "a.jpg");
        var fileB = Path.Combine(_tempDirectory, "b.png");
        File.WriteAllText(fileA, "a");
        File.WriteAllText(fileB, "b");

        var store = new InMemorySettingsStore();
        var service = new SettingsService(store);
        var snapshot = new AppSettingsSnapshot
        {
            Quality = 72,
            OutputFormat = "webp",
            RenameEnabled = true,
            RenamePrefix = "holiday",
            RenameSeparator = "_",
            RenameStartNumber = 9,
            LastDirectory = _tempDirectory,
            LastFiles = [fileA, fileB],
            LastMode = "files",
        };

        await service.SaveAsync(snapshot);
        var loaded = await service.LoadAsync();

        Assert.Equal(snapshot.Quality, loaded.Quality);
        Assert.Equal(snapshot.OutputFormat, loaded.OutputFormat);
        Assert.Equal(snapshot.RenameEnabled, loaded.RenameEnabled);
        Assert.Equal(snapshot.RenamePrefix, loaded.RenamePrefix);
        Assert.Equal(snapshot.RenameSeparator, loaded.RenameSeparator);
        Assert.Equal(snapshot.RenameStartNumber, loaded.RenameStartNumber);
        Assert.Equal(snapshot.LastDirectory, loaded.LastDirectory);
        Assert.Equal(snapshot.LastMode, loaded.LastMode);
        Assert.Equal(snapshot.LastFiles.Count, loaded.LastFiles.Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}

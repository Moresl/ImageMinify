using ImageMinify.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageMinify.Tests;

public sealed class PngCompressorTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-png-{Guid.NewGuid():N}");

    public PngCompressorTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Compress_CreatesOptimizedPngAndStripsExif()
    {
        var inputPath = Path.Combine(_tempDirectory, "input.png");
        var outputPath = Path.Combine(_tempDirectory, "output.png");

        using (var image = new Image<Rgba32>(96, 96))
        {
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    image[x, y] = new Rgba32((byte)(x * 2), (byte)(y * 2), 120, (byte)(255 - x));
                }
            }

            var exif = new ExifProfile();
            exif.SetValue(ExifTag.Software, "ShouldBeRemoved");
            image.Metadata.ExifProfile = exif;
            image.Save(inputPath);
        }

        new PngCompressor(new ExifService()).Compress(inputPath, outputPath, 70);

        Assert.True(File.Exists(outputPath));

        using var output = Image.Load(outputPath);
        Assert.Null(output.Metadata.ExifProfile);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}

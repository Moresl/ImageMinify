using ImageMinify.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageMinify.Tests;

public sealed class JpegCompressorTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"imageminify-jpeg-{Guid.NewGuid():N}");

    public JpegCompressorTests()
    {
        System.IO.Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Compress_PreservesReducedExifSetOnly()
    {
        var inputPath = Path.Combine(_tempDirectory, "input.jpg");
        var outputPath = Path.Combine(_tempDirectory, "output.jpg");

        using (var image = new Image<Rgba32>(64, 64, Color.Red))
        {
            var exif = new ExifProfile();
            exif.SetValue(ExifTag.Make, "Canon");
            exif.SetValue(ExifTag.Model, "R6");
            exif.SetValue(ExifTag.Software, "Original");
            exif.SetValue(ExifTag.DateTime, "2026:03:16 10:11:12");
            exif.SetValue(ExifTag.Artist, "RemoveMe");
            exif.SetValue(ExifTag.Orientation, (ushort)6);
            image.Metadata.ExifProfile = exif;
            image.Save(inputPath);
        }

        new JpegCompressor(new ExifService()).Compress(inputPath, outputPath, 82);

        Assert.True(File.Exists(outputPath));

        var directories = ImageMetadataReader.ReadMetadata(outputPath);
        var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        Assert.NotNull(ifd0);
        Assert.Equal("Canon", ifd0!.GetString(ExifDirectoryBase.TagMake));
        Assert.Equal("R6", ifd0.GetString(ExifDirectoryBase.TagModel));
        Assert.Equal("Original", ifd0.GetString(ExifDirectoryBase.TagSoftware));
        Assert.Equal("2026:03:16 10:11:12", ifd0.GetString(ExifDirectoryBase.TagDateTime));
        Assert.Equal(1, ifd0.GetInt32(ExifDirectoryBase.TagOrientation));
        Assert.Null(ifd0.GetString(ExifDirectoryBase.TagArtist));
    }

    public void Dispose()
    {
        if (System.IO.Directory.Exists(_tempDirectory))
        {
            System.IO.Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}

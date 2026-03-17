using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageMinify.Services;

public sealed class WebpCompressor
{
    private readonly ExifService _exifService;

    public WebpCompressor(ExifService exifService)
    {
        _exifService = exifService;
    }

    public void Compress(string inputPath, string outputPath, int quality)
    {
        using var image = Image.Load<Rgba32>(inputPath);
        _exifService.NormalizeForNonJpegOutput(image);

        image.Save(outputPath, new WebpEncoder
        {
            Quality = quality,
            Method = WebpEncodingMethod.BestQuality,
        });
    }
}

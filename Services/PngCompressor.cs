using System.Diagnostics;
using ImageMinify.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace ImageMinify.Services;

public sealed class PngCompressor
{
    private readonly ExifService _exifService;
    private readonly ImagequantNativeQuantizer _imagequantNativeQuantizer = new();

    public PngCompressor(ExifService exifService)
    {
        _exifService = exifService;
    }

    public bool HasImagequant => _imagequantNativeQuantizer.IsAvailable;

    public bool HasOxipng => NativeLibraryLoader.ResolveToolPath("oxipng.exe", "oxipng") is not null;

    public string QuantizationName => HasImagequant ? "imagequant" : "ImageSharp fallback";

    public string LosslessOptimizerName => HasOxipng ? "oxipng" : "disabled";

    public void Compress(string inputPath, string outputPath, int quality)
    {
        using var image = Image.Load<Rgba32>(inputPath);
        _exifService.NormalizeForNonJpegOutput(image);

        var encoder = new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestCompression,
            ColorType = PngColorType.Palette,
            Quantizer = CreateQuantizer(image, quality),
        };

        image.Save(outputPath, encoder);

        OptimizeLossless(outputPath);
    }

    private IQuantizer CreateQuantizer(Image<Rgba32> image, int quality)
    {
        if (_imagequantNativeQuantizer.TryCreateQuantizer(image, quality, out var quantizer) && quantizer is not null)
        {
            return quantizer;
        }

        return new OctreeQuantizer(new QuantizerOptions
        {
            MaxColors = Math.Clamp(32 + (int)(224 * quality / 100D), 32, 256),
        });
    }

    private void OptimizeLossless(string outputPath)
    {
        var toolPath = NativeLibraryLoader.ResolveToolPath("oxipng.exe", "oxipng");
        if (toolPath is null)
        {
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                ArgumentList =
                {
                    "-o",
                    "4",
                    "--strip",
                    "all",
                    outputPath,
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            using var process = Process.Start(startInfo);

            process?.WaitForExit(60000);
        }
        catch
        {
            // oxipng is optional and should never fail the main compression flow.
        }
    }
}

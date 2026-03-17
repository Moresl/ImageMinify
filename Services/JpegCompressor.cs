using System.Diagnostics;
using ImageMinify.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageMinify.Services;

public sealed class JpegCompressor
{
    private readonly ExifService _exifService;
    private readonly string? _optimizerToolPath;

    public JpegCompressor(ExifService exifService)
    {
        _exifService = exifService;
        _optimizerToolPath = ResolveOptimizerToolPath();
    }

    public bool HasMozJpeg => !string.IsNullOrWhiteSpace(_optimizerToolPath);

    public string OptimizerName => HasMozJpeg ? "MozJPEG" : "ImageSharp fallback";

    public void Compress(string inputPath, string outputPath, int quality)
    {
        using var image = Image.Load<Rgba32>(inputPath);
        _exifService.NormalizeForJpegOutput(image, inputPath);

        using var canvas = new Image<Rgba32>(image.Width, image.Height, Color.White);
        canvas.Mutate(context => context.DrawImage(image, 1F));
        canvas.Metadata.ExifProfile = image.Metadata.ExifProfile is null
            ? null
            : new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile(image.Metadata.ExifProfile.ToByteArray());
        canvas.Save(outputPath, new JpegEncoder
        {
            Quality = quality,
            Interleaved = true,
            SkipMetadata = false,
        });

        TryOptimizeLossless(outputPath);
    }

    private void TryOptimizeLossless(string outputPath)
    {
        var toolPath = _optimizerToolPath;
        if (toolPath is null)
        {
            return;
        }

        var tempPath = Path.Combine(Path.GetDirectoryName(outputPath) ?? AppContext.BaseDirectory, $"{Path.GetFileNameWithoutExtension(outputPath)}.mozopt.jpg");
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = toolPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            foreach (var argument in BuildOptimizerArguments(toolPath, tempPath, outputPath))
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return;
            }

            process.WaitForExit(10000);
            if (process is { ExitCode: 0 } && File.Exists(tempPath))
            {
                File.Copy(tempPath, outputPath, overwrite: true);
            }
        }
        catch
        {
            // Lossless optimization is optional; keep the ImageSharp output when it fails.
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string? ResolveOptimizerToolPath()
    {
        return NativeLibraryLoader.ResolveToolPath("jpegtran.exe", "jpegtran", "mozjpeg.exe", "mozjpeg");
    }

    private static IEnumerable<string> BuildOptimizerArguments(string toolPath, string tempPath, string outputPath)
    {
        return ["-optimize", "-copy", "all", "-outfile", tempPath, outputPath];
    }
}

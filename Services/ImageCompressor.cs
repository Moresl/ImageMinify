using ImageMinify.Helpers;
using ImageMinify.Models;
using SixLabors.ImageSharp;

namespace ImageMinify.Services;

public sealed class ImageCompressor : IImageCompressor
{
    private static readonly IReadOnlyList<string> Extensions = [".jpg", ".jpeg", ".png", ".bmp", ".webp"];

    private readonly JpegCompressor _jpegCompressor;
    private readonly PngCompressor _pngCompressor;
    private readonly WebpCompressor _webpCompressor;

    public ImageCompressor(
        JpegCompressor jpegCompressor,
        PngCompressor pngCompressor,
        WebpCompressor webpCompressor)
    {
        _jpegCompressor = jpegCompressor;
        _pngCompressor = pngCompressor;
        _webpCompressor = webpCompressor;
    }

    public IReadOnlyList<string> SupportedExtensions => Extensions;

    public Task<CompressionResult> CompressAsync(
        string inputPath,
        string? outputPath = null,
        string outputFormat = "original",
        int quality = 85,
        CancellationToken ct = default)
    {
        return Task.Run(() => CompressInternal(inputPath, outputPath, outputFormat, quality, ct), ct);
    }

    public Task<(List<CompressionResult> Results, CompressionSummary Summary)> CompressDirectoryAsync(
        string directoryPath,
        string outputFormat = "original",
        int quality = 85,
        IProgress<(int Processed, int Total, CompressionResult? Result)>? progress = null,
        CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var imageFiles = Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
                .Where(IsSupportedFormat)
                .ToList();

            var results = new List<CompressionResult>(imageFiles.Count);

            for (var index = 0; index < imageFiles.Count; index++)
            {
                ct.ThrowIfCancellationRequested();

                var result = CompressInternal(imageFiles[index], null, outputFormat, quality, ct);
                results.Add(result);
                progress?.Report((index + 1, imageFiles.Count, result));
            }

            var summary = CompressionSummary.FromResults(imageFiles.Count, imageFiles.Count, results);
            return (results, summary);
        }, ct);
    }

    public EngineCapabilities GetCapabilities()
    {
        return new EngineCapabilities
        {
            HasMozJpeg = _jpegCompressor.HasMozJpeg,
            HasImagequant = _pngCompressor.HasImagequant,
            HasOxipng = _pngCompressor.HasOxipng,
            PngQuantization = _pngCompressor.QuantizationName,
            JpegOptimizer = _jpegCompressor.OptimizerName,
            PngLossless = _pngCompressor.LosslessOptimizerName,
        };
    }

    public bool IsSupportedFormat(string path)
    {
        var extension = Path.GetExtension(path);
        return Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private CompressionResult CompressInternal(
        string inputPath,
        string? outputPath,
        string outputFormat,
        int quality,
        CancellationToken ct)
    {
        if (!IsSupportedFormat(inputPath))
        {
            return new CompressionResult
            {
                OriginalPath = inputPath,
                Success = false,
                Error = "Unsupported image format.",
            };
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            var outputExtension = ResolveOutputExtension(Path.GetExtension(inputPath), outputFormat);
            outputPath ??= BuildOutputPath(inputPath, outputExtension);

            var originalSize = new FileInfo(inputPath).Length;
            CompressToFormat(inputPath, outputPath, outputExtension, quality);

            var compressedSize = new FileInfo(outputPath).Length;
            var ratio = originalSize == 0 ? 0 : (1 - (double)compressedSize / originalSize) * 100;

            return new CompressionResult
            {
                OriginalPath = inputPath,
                CompressedPath = outputPath,
                OriginalSize = originalSize,
                CompressedSize = compressedSize,
                OriginalSizeFormatted = FileSizeFormatter.Format(originalSize),
                CompressedSizeFormatted = FileSizeFormatter.Format(compressedSize),
                CompressionRatio = ratio,
                Success = true,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new CompressionResult
            {
                OriginalPath = inputPath,
                CompressedPath = outputPath ?? string.Empty,
                Success = false,
                Error = ex.Message,
            };
        }
    }

    private void CompressToFormat(string inputPath, string outputPath, string outputExtension, int quality)
    {
        switch (outputExtension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                _jpegCompressor.Compress(inputPath, outputPath, quality);
                break;
            case ".png":
                _pngCompressor.Compress(inputPath, outputPath, quality);
                break;
            case ".webp":
                _webpCompressor.Compress(inputPath, outputPath, quality);
                break;
            default:
                using (var image = Image.Load(inputPath))
                {
                    image.Save(outputPath);
                }
                break;
        }
    }

    private static string ResolveOutputExtension(string inputExtension, string outputFormat)
    {
        return outputFormat.ToLowerInvariant() switch
        {
            "jpeg" => ".jpg",
            "png" => ".png",
            "webp" => ".webp",
            _ => inputExtension,
        };
    }

    private static string BuildOutputPath(string inputPath, string outputExtension)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? AppContext.BaseDirectory;
        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(directory, $"{fileName}_compressed{outputExtension}");
    }
}

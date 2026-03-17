using ImageMinify.Models;

namespace ImageMinify.Services;

public interface IImageCompressor
{
    IReadOnlyList<string> SupportedExtensions { get; }

    Task<CompressionResult> CompressAsync(
        string inputPath,
        string? outputPath = null,
        string outputFormat = "original",
        int quality = 85,
        CancellationToken ct = default);

    Task<(List<CompressionResult> Results, CompressionSummary Summary)> CompressDirectoryAsync(
        string directoryPath,
        string outputFormat = "original",
        int quality = 85,
        IProgress<(int Processed, int Total, CompressionResult? Result)>? progress = null,
        CancellationToken ct = default);

    EngineCapabilities GetCapabilities();
}

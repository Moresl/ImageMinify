using System.IO;

namespace ImageMinify.Models;

public record CompressionResult
{
    public string OriginalPath { get; init; } = string.Empty;

    public string CompressedPath { get; init; } = string.Empty;

    public long OriginalSize { get; init; }

    public long CompressedSize { get; init; }

    public string OriginalSizeFormatted { get; init; } = "0 B";

    public string CompressedSizeFormatted { get; init; } = "0 B";

    public double CompressionRatio { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }

    public string FileName => Path.GetFileName(OriginalPath);

    public string CompressionRatioDisplay => $"{CompressionRatio:F2}%";
}

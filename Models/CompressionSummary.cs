using ImageMinify.Helpers;

namespace ImageMinify.Models;

public record CompressionSummary
{
    public int TotalFiles { get; init; }

    public int ProcessedFiles { get; init; }

    public long TotalOriginalSize { get; init; }

    public long TotalCompressedSize { get; init; }

    public string TotalOriginalSizeFormatted { get; init; } = "0 B";

    public string TotalCompressedSizeFormatted { get; init; } = "0 B";

    public double OverallCompressionRatio { get; init; }

    public string TotalFilesDisplay => $"总文件数: {TotalFiles}";

    public string TotalOriginalSizeDisplay => $"压缩前总大小: {TotalOriginalSizeFormatted}";

    public string TotalCompressedSizeDisplay => $"压缩后总大小: {TotalCompressedSizeFormatted}";

    public string OverallCompressionRatioDisplay => $"总体压缩比例: {OverallCompressionRatio:F2}%";

    public static CompressionSummary FromResults(int totalFiles, int processedFiles, IEnumerable<CompressionResult> results)
    {
        var successfulResults = results.Where(result => result.Success).ToList();
        var totalOriginalSize = successfulResults.Sum(result => result.OriginalSize);
        var totalCompressedSize = successfulResults.Sum(result => result.CompressedSize);
        var overallRatio = totalOriginalSize == 0
            ? 0
            : (1 - (double)totalCompressedSize / totalOriginalSize) * 100;

        return new CompressionSummary
        {
            TotalFiles = totalFiles,
            ProcessedFiles = processedFiles,
            TotalOriginalSize = totalOriginalSize,
            TotalCompressedSize = totalCompressedSize,
            TotalOriginalSizeFormatted = FileSizeFormatter.Format(totalOriginalSize),
            TotalCompressedSizeFormatted = FileSizeFormatter.Format(totalCompressedSize),
            OverallCompressionRatio = overallRatio,
        };
    }
}

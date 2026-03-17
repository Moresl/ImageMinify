namespace ImageMinify.Models;

public record EngineCapabilities
{
    public bool HasMozJpeg { get; init; }

    public bool HasImagequant { get; init; }

    public bool HasOxipng { get; init; }

    public string PngQuantization { get; init; } = "ImageSharp fallback";

    public string JpegOptimizer { get; init; } = "ImageSharp fallback";

    public string PngLossless { get; init; } = "disabled";

    public string EngineSummary =>
        $"MozJPEG {(HasMozJpeg ? "✓" : "×")} | imagequant {(HasImagequant ? "✓" : "×")} | oxipng {(HasOxipng ? "✓" : "×")}";

    public string JpegOptimizerDisplay => $"JPEG 优化: {JpegOptimizer}";

    public string PngQuantizationDisplay => $"PNG 量化: {PngQuantization}";

    public string PngLosslessDisplay => $"PNG 无损: {PngLossless}";
}

namespace ImageMinify.Models;

public record AppSettingsSnapshot
{
    public int Quality { get; init; } = 85;

    public string OutputFormat { get; init; } = "original";

    public bool RenameEnabled { get; init; }

    public string RenamePrefix { get; init; } = string.Empty;

    public string RenameSeparator { get; init; } = "-";

    public int RenameStartNumber { get; init; } = 1;

    public string LastDirectory { get; init; } = string.Empty;

    public IReadOnlyList<string> LastFiles { get; init; } = Array.Empty<string>();

    public string LastMode { get; init; } = string.Empty;
}

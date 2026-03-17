namespace ImageMinify.Models;

public record RenameSettings
{
    public bool Enabled { get; init; }

    public string Prefix { get; init; } = string.Empty;

    public string Separator { get; init; } = "-";

    public int StartNumber { get; init; } = 1;
}

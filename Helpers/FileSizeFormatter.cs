namespace ImageMinify.Helpers;

public static class FileSizeFormatter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string Format(long sizeInBytes)
    {
        double value = sizeInBytes;
        var unitIndex = 0;

        while (value >= 1024 && unitIndex < Units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:F2} {Units[unitIndex]}";
    }
}

using ImageMinify.Models;

namespace ImageMinify.Services;

public sealed class FileRenameService
{
    public CompressionResult ApplyRename(CompressionResult result, RenameSettings settings, int processed, int total)
    {
        if (!settings.Enabled || !result.Success || string.IsNullOrWhiteSpace(result.CompressedPath))
        {
            return result;
        }

        var directory = Path.GetDirectoryName(result.CompressedPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return result;
        }

        var extension = Path.GetExtension(result.CompressedPath);
        var targetPath = Path.Combine(directory, BuildFileName(settings, processed, total, extension));
        targetPath = EnsureUniquePath(targetPath);

        if (string.Equals(result.CompressedPath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        try
        {
            File.Move(result.CompressedPath, targetPath, overwrite: false);
            return result with { CompressedPath = targetPath };
        }
        catch
        {
            return result;
        }
    }

    public string BuildFileName(RenameSettings settings, int processed, int total, string extension)
    {
        var number = settings.StartNumber + processed - 1;
        var lastNumber = settings.StartNumber + Math.Max(total - 1, 0);
        var digits = Math.Max(1, lastNumber.ToString().Length);
        var formattedNumber = number.ToString($"D{digits}");
        return $"{settings.Prefix}{settings.Separator}{formattedNumber}{extension}";
    }

    private static string EnsureUniquePath(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
        var baseName = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        for (var index = 1; index < 1000; index++)
        {
            var candidate = Path.Combine(directory, $"{baseName}_{index}{extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(directory, $"{baseName}_{Guid.NewGuid():N}{extension}");
    }
}

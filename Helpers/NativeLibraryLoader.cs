using System.Runtime.InteropServices;

namespace ImageMinify.Helpers;

public static class NativeLibraryLoader
{
    public static string? ResolveLibraryPath(params string[] names)
    {
        return ResolveToolPath(names);
    }

    public static bool ExistsNearApplication(string fileName)
    {
        var baseDirectory = AppContext.BaseDirectory;
        return File.Exists(Path.Combine(baseDirectory, fileName))
            || File.Exists(Path.Combine(baseDirectory, "bin", fileName));
    }

    public static bool TryLoadOptional(string fileName, out nint handle)
    {
        handle = IntPtr.Zero;

        foreach (var candidate in GetCandidates(fileName))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            if (NativeLibrary.TryLoad(candidate, out handle))
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryLoadOptional(out nint handle, params string[] fileNames)
    {
        handle = IntPtr.Zero;

        foreach (var fileName in fileNames)
        {
            if (TryLoadOptional(fileName, out handle))
            {
                return true;
            }
        }

        return false;
    }

    public static string? ResolveToolPath(params string[] names)
    {
        foreach (var name in names)
        {
            foreach (var candidate in GetCandidates(name))
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            var pathValue = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                continue;
            }

            foreach (var segment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(segment.Trim(), name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetCandidates(string fileName)
    {
        var baseDirectory = AppContext.BaseDirectory;
        yield return Path.Combine(baseDirectory, fileName);
        yield return Path.Combine(baseDirectory, "bin", fileName);
        yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", fileName);
    }
}

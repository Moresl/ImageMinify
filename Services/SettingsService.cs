using System.Text.Json;
using ImageMinify.Models;
using System.Windows;

namespace ImageMinify.Services;

public sealed class SettingsService
{
    private readonly ISettingsStore _store;

    public SettingsService()
        : this(new WindowsSettingsStore())
    {
    }

    public SettingsService(ISettingsStore store)
    {
        _store = store;
    }

    public Task<AppSettingsSnapshot> LoadAsync()
    {
        var filesJson = _store.GetValue("SelectLastFiles") as string ?? "[]";
        IReadOnlyList<string> lastFiles;

        try
        {
            lastFiles = JsonSerializer.Deserialize<List<string>>(filesJson) ?? [];
        }
        catch
        {
            lastFiles = [];
        }

        return Task.FromResult(new AppSettingsSnapshot
        {
            Quality = ReadInt("CompressQuality", 85),
            OutputFormat = ReadString("CompressOutputFormat", "original"),
            RenameEnabled = ReadBool("RenameEnabled"),
            RenamePrefix = ReadString("RenamePrefix", string.Empty),
            RenameSeparator = ReadString("RenameSeparator", "-"),
            RenameStartNumber = ReadInt("RenameStartNumber", 1),
            LastDirectory = ReadString("SelectLastDirectory", string.Empty),
            LastFiles = lastFiles.Where(File.Exists).ToList(),
            LastMode = ReadString("SelectLastMode", string.Empty),
        });
    }

    public Task SaveAsync(AppSettingsSnapshot snapshot)
    {
        _store.SetValue("CompressQuality", snapshot.Quality);
        _store.SetValue("CompressOutputFormat", snapshot.OutputFormat);
        _store.SetValue("RenameEnabled", snapshot.RenameEnabled);
        _store.SetValue("RenamePrefix", snapshot.RenamePrefix);
        _store.SetValue("RenameSeparator", snapshot.RenameSeparator);
        _store.SetValue("RenameStartNumber", snapshot.RenameStartNumber);
        _store.SetValue("SelectLastDirectory", snapshot.LastDirectory);
        _store.SetValue("SelectLastFiles", JsonSerializer.Serialize(snapshot.LastFiles));
        _store.SetValue("SelectLastMode", snapshot.LastMode);
        return Task.CompletedTask;
    }

    public void RestoreWindowPlacement(Window window)
    {
        var width = ReadDouble("WindowWidth", 1120);
        var height = ReadDouble("WindowHeight", 820);
        var x = ReadDouble("WindowX", 120);
        var y = ReadDouble("WindowY", 120);

        window.Width = width;
        window.Height = height;
        window.Left = x;
        window.Top = y;
    }

    public void SaveWindowPlacement(Window window)
    {
        _store.SetValue("WindowWidth", window.Width);
        _store.SetValue("WindowHeight", window.Height);
        _store.SetValue("WindowX", window.Left);
        _store.SetValue("WindowY", window.Top);
    }

    private int ReadInt(string key, int defaultValue)
    {
        return _store.GetValue(key) switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            _ => defaultValue,
        };
    }

    private bool ReadBool(string key, bool defaultValue = false)
    {
        return _store.GetValue(key) switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            int intValue => intValue != 0,
            _ => defaultValue,
        };
    }

    private double ReadDouble(string key, double defaultValue)
    {
        return _store.GetValue(key) switch
        {
            double doubleValue => doubleValue,
            float floatValue => floatValue,
            int intValue => intValue,
            long longValue => longValue,
            string stringValue when double.TryParse(stringValue, out var parsed) => parsed,
            _ => defaultValue,
        };
    }

    private string ReadString(string key, string defaultValue)
    {
        return _store.GetValue(key) as string ?? defaultValue;
    }
}

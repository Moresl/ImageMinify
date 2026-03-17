using System.Text.Json;

namespace ImageMinify.Services;

public sealed class WindowsSettingsStore : ISettingsStore
{
    private readonly string _settingsPath;
    private Dictionary<string, JsonElement> _values;

    public WindowsSettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDirectory = Path.Combine(appData, "ImageMinify");
        Directory.CreateDirectory(settingsDirectory);

        _settingsPath = Path.Combine(settingsDirectory, "settings.json");
        _values = LoadValues(_settingsPath);
    }

    public object? GetValue(string key)
    {
        return !_values.TryGetValue(key, out var value)
            ? null
            : value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number when value.TryGetInt32(out var intValue) => intValue,
                JsonValueKind.Number when value.TryGetInt64(out var longValue) => longValue,
                JsonValueKind.Number when value.TryGetDouble(out var doubleValue) => doubleValue,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => value.GetRawText(),
            };
    }

    public void SetValue(string key, object value)
    {
        _values[key] = JsonSerializer.SerializeToElement(value);
        SaveValues();
    }

    private void SaveValues()
    {
        var json = JsonSerializer.Serialize(_values, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        File.WriteAllText(_settingsPath, json);
    }

    private static Dictionary<string, JsonElement> LoadValues(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

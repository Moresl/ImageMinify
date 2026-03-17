using ImageMinify.Services;

namespace ImageMinify.Tests;

internal sealed class InMemorySettingsStore : ISettingsStore
{
    private readonly Dictionary<string, object> _values = [];

    public object? GetValue(string key)
    {
        return _values.TryGetValue(key, out var value) ? value : null;
    }

    public void SetValue(string key, object value)
    {
        _values[key] = value;
    }
}

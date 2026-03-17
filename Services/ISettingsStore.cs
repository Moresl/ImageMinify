namespace ImageMinify.Services;

public interface ISettingsStore
{
    object? GetValue(string key);

    void SetValue(string key, object value);
}

namespace NitroxModel.Helper;

public interface IKeyValueStore
{
    /// <summary>
    ///     Gets a value for a key.
    /// </summary>
    /// <param name="key">Key to get value of.</param>
    /// <param name="defaultValue">Default value to return if key does not exist or type conversion failed.</param>
    /// <returns>The value or null if key was not found or conversion failed.</returns>
    T GetValue<T>(string key, T defaultValue = default);

    /// <summary>
    ///     Sets a value for a key.
    /// </summary>
    /// <param name="key">Key to set value of.</param>
    /// <param name="value">Value to set for the key.</param>
    /// <returns>True if the value was found.</returns>
    bool SetValue<T>(string key, T value);

    /// <summary>
    ///     Deletes a key along with its value.
    /// </summary>
    /// <param name="key">Key to delete.</param>
    /// <returns>True if the key was deleted.</returns>
    bool DeleteKey(string key);

    /// <summary>
    ///     Check if a key exists.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>True if the key exists.</returns>
    bool KeyExists(string key);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NitroxModel.Helper;

namespace NitroxModel.Platforms.OS.Shared;

public class ConfigFileKeyValueStore : IKeyValueStore
{
    private bool hasLoaded = false;
    private readonly Dictionary<string, object> keyValuePairs = new();
    public string FolderPath { get; }
    public string FilePath => Path.Combine(FolderPath, "nitrox.cfg");

    public ConfigFileKeyValueStore()
    {
        // LocalApplicationData's default is $HOME/.config under linux and XDG_CONFIG_HOME if set
        // What is the difference between .config and .local/share?
        // .config should contain all config files.
        // .local/share should contain data that isn't config files (binary blobs, downloaded data, server saves).
        // .cache should house all cache files (files that can be safely deleted to free up space)
        string localShare = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(localShare))
        {
            throw new Exception("Could not determine where to save configs. Check HOME and XDG_CONFIG_HOME variables.");
        }
        FolderPath = Path.Combine(localShare, "Nitrox");
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        TryLoadConfig();
        bool succeeded = keyValuePairs.TryGetValue(key, out object obj);
        if (!succeeded)
        {
            return defaultValue;
        }
        if (obj is JsonElement element)
        {
            // System.Text.Json stores objects as JsonElement
            try
            {
                return element.Deserialize<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        // if a value has been added at runtime and not deserialized, it should be casted directly
        try
        {
            return (T)obj;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public bool SetValue<T>(string key, T value)
    {
        TryLoadConfig();
        keyValuePairs[key] = value;
        TrySaveConfig();
        return true;
    }

    public (bool success, Exception error) TrySaveConfig()
    {
        // saving configs isn't critical, if it fails the values will still exists at runtime, but won't be loaded the next time you start up Nitrox.
        try
        {
            // Create directories if they don't already exist
            Directory.CreateDirectory(FolderPath);

            // serialize the keyValuePairs
            string serialized = JsonSerializer.Serialize(keyValuePairs, new JsonSerializerOptions { WriteIndented = true });

            // try to write the file
            File.WriteAllText(FilePath, serialized);
            return (true, null);
        }
        catch (Exception e)
        {
            return (false, e);
        }
    }

    private (bool success, Exception error) TryLoadConfig()
    {
        if (hasLoaded)
        {
            return (true, null);
        }

        // Check if config file exists
        bool configExists = File.Exists(FilePath);
        
        Dictionary<string, object> deserialized = new();
        if (configExists)
        {
            try
            {
                deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(FilePath));
                if (deserialized == null)
                {
                    return (false, new Exception("Deserialized object was null"));
                }
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        foreach (KeyValuePair<string, object> item in deserialized)
        {
            keyValuePairs[item.Key] = item.Value;
        }

        // Initialize default settings for first-time users (when no config file exists)
        if (!configExists)
        {
            InitializeDefaultSettings();
        }

        hasLoaded = true;
        return (true, null);
    }

    private void InitializeDefaultSettings()
    {
        if (!keyValuePairs.ContainsKey("IsSteamOverlayEnabled"))
        {
            keyValuePairs["IsSteamOverlayEnabled"] = false;
        }
        
        if (!keyValuePairs.ContainsKey("IsBigPictureModeEnabled"))
        {
            keyValuePairs["IsBigPictureModeEnabled"] = false;
        }
        
        if (!keyValuePairs.ContainsKey("IsMultipleGameInstancesAllowed"))
        {
            keyValuePairs["IsMultipleGameInstancesAllowed"] = false;
        }
        
        TrySaveConfig();
    }

    public bool DeleteKey(string key)
    {
        if (!keyValuePairs.Remove(key))
        {
            return false;
        }
        TrySaveConfig();
        return true;
    }

    public bool KeyExists(string key) => keyValuePairs.ContainsKey(key);
}

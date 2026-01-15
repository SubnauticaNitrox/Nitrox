using System;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Platforms.OS.Windows;

#if MODERN_DOTNET
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
public class RegistryKeyValueStore : IKeyValueStore
{
    public static string KeyToRegistryPath(string key) => @$"SOFTWARE\Nitrox\{key}";

    public T GetValue<T>(string key, T defaultValue) => RegistryEx.Read(KeyToRegistryPath(key), defaultValue);

    public bool SetValue<T>(string key, T value)
    {
        try
        {
            RegistryEx.Write(KeyToRegistryPath(key), value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool DeleteKey(string key) => RegistryEx.Delete(KeyToRegistryPath(key));

    public bool KeyExists(string key) => RegistryEx.Exists(KeyToRegistryPath(key));
}

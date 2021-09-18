using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    public static class RegistryEx
    {
        public static T Read<T>(string path, string key)
        {
            using RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path);
            return subkey?.GetValue(key) is T typedValue ? typedValue : default;
        }

        /// <summary>
        ///     Waits for a registry value to have the given value.
        /// </summary>
        public static Task CompareAsync<T>(string path, string key, Func<T, bool> predicate, CancellationToken token)
        {
            static bool Test(RegistryKey regKey, string regKeyName, Func<T, bool> testPredicate)
            {
                T preTestVal = regKey?.GetValue(regKeyName) is T typedValue ? typedValue : default;
                return testPredicate(preTestVal);
            }
            
            if (token == default)
            {
                CancellationTokenSource source = new(TimeSpan.FromSeconds(10));
                token = source.Token;
            }
            
            // Test once before in-case it is already successful.
            using RegistryKey preTestKey = Registry.CurrentUser.OpenSubKey(path);
            if (Test(preTestKey, key, predicate))
            {
                return Task.CompletedTask;
            }
            
            // Wait for predicate to be successful.
            return Task.Run(async () =>
                            {
                                using RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path);
                                while (!token.IsCancellationRequested)
                                {
                                    if (!Test(subkey, key, predicate))
                                    {
                                        await Task.Delay(100, token);
                                        continue;
                                    }

                                    break;
                                }
                            },
                            token);
        }

        public static Task CompareAsync<T>(string path, string key, Func<T, bool> predicate, TimeSpan timeout = default)
        {
            CancellationTokenSource source = new(timeout == default ? TimeSpan.FromSeconds(10) : timeout);
            return CompareAsync(path, key, predicate, source.Token);
        }
    }
}

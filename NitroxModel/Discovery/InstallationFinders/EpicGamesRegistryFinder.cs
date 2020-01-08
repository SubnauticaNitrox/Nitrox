using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class EpicGamesRegistryFinder : IFindGameInstallation
    {
        private const string BASE_REGISTRY_KEY = @"Local Settings\Software\Microsoft\Windows\Shell\MuiCache";

        public Optional<string> FindGame(List<string> errors = null)
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(BASE_REGISTRY_KEY))
            {
                if (key == null)
                {
                    errors?.Add($"Could not find Win32 registry key '{BASE_REGISTRY_KEY}' to find Subnautica installation for Epic Games' launcher.");
                    return Optional<string>.Empty();
                }

                string subnauticaKeyName = key.GetValueNames().FirstOrDefault(name => name.IndexOf("subnautica.exe", StringComparison.OrdinalIgnoreCase) >= 0);
                if (string.IsNullOrEmpty(subnauticaKeyName))
                {
                    errors?.Add($"Registry key '{Path.Combine(key.Name, "subnautica.exe")}' does not exist. Can not determine Subnautica installation directory from Epic Games' launcher.");
                    return Optional<string>.Empty();
                }

                // Replace FriendAppName if added to key on Win7+EpicGames Subnautica installation.
                string guessedInstallDirectory = Directory.GetParent(subnauticaKeyName.Replace(".FriendlyAppName", "")).FullName;

                if (!Directory.Exists(guessedInstallDirectory))
                {
                    errors?.Add($"Path '{guessedInstallDirectory}' found in registry key for Epic Games' launcher is not a directory.");
                    return Optional<string>.Empty();
                }

                if (!Directory.GetFiles(guessedInstallDirectory, "*.exe").Any(fileName => fileName.IndexOf("subnautica.exe", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    errors?.Add($"Path '{guessedInstallDirectory}' found in registry key for Epic Games' launcher does not contain the Subnautica executable.");
                    return Optional<string>.Empty();
                }

                return Optional<string>.Of(guessedInstallDirectory);
            }
        }
    }
}

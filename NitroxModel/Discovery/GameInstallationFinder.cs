using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Discovery.InstallationFinders;

namespace NitroxModel.Discovery
{
    /// <summary>
    ///     Main game installation finder that will use all available methods of detection to find the Subnautica installation
    ///     directory.
    /// </summary>
    public class GameInstallationFinder : IFindGameInstallation
    {
        private static readonly Lazy<GameInstallationFinder> instance = new(() => new GameInstallationFinder());
        public static GameInstallationFinder Instance => instance.Value;

        /// <summary>
        ///     The order of these finders is VERY important. Only change if you know what you're doing.
        /// </summary>
        private readonly IFindGameInstallation[] finders = {
            new GameInCurrentDirectoryFinder(),
            new ConfigGameFinder(),
            new SteamGameRegistryFinder(),
            new EpicGamesInstallationFinder(),
            new DiscordGameFinder(),
            new EnvironmentGameFinder()
        };

        public string FindGame(IList<string> errors = null)
        {
            errors ??= new List<string>();
            foreach (IFindGameInstallation finder in finders)
            {
                string path = finder.FindGame(errors);
                if (path == null)
                {
                    continue;
                }

                errors.Clear();
                return Path.GetFullPath(path);
            }

            return null;
        }

        public static bool IsSubnauticaDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            return Directory.EnumerateFiles(directory, "*.exe")
                .Any(file => Path.GetFileName(file)?.Equals("subnautica.exe", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}

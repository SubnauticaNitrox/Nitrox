using System;
using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery.InstallationFinders;

namespace NitroxModel.Discovery
{
    /// <summary>
    ///     Main game installation finder that will use all available methods of detection to find the Subnautica installation
    ///     directory.
    /// </summary>
    public class GameInstallationFinder : IFindGameInstallation
    {
        /// <summary>
        ///     The order of these finders is VERY important. Only change if you know what you're doing.
        /// </summary>
        private readonly IFindGameInstallation[] finders = {
            new GameInCurrentDirectoryFinder(),
            new ConfigFileGameFinder(),
            new SteamGameRegistryFinder(),
            new EpicGamesInstallationFinder(),
        };
        
        /// <summary>
        /// Thread safe backing field for Singleton instance.
        /// </summary>
        private static readonly Lazy<GameInstallationFinder> instance = new Lazy<GameInstallationFinder>(() => new GameInstallationFinder());
        public static GameInstallationFinder Instance => instance.Value;

        public Optional<string> FindGame(List<string> errors = null)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }
            foreach (IFindGameInstallation finder in finders)
            {
                Optional<string> path = finder.FindGame(errors);
                if (!path.HasValue)
                {
                    continue;
                }
                
                errors?.Clear();
                return Path.GetFullPath(path.Value); // Make path separators consistent
            }

            return Optional.Empty;
        }
    }
}

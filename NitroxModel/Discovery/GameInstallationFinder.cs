using System;
using System.Collections.Generic;
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
            new EpicGamesInstallationFinder(),
            new SteamGameRegistryFinder(),
        };
        
        /// <summary>
        /// Thread safe backing field for Singleton instance.
        /// </summary>
        private static readonly Lazy<GameInstallationFinder> instance = new Lazy<GameInstallationFinder>(() => new GameInstallationFinder());
        public static GameInstallationFinder Instance => instance.Value;

        public string FindGame(List<string> errors = null)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }
            foreach (IFindGameInstallation finder in finders)
            {
                string path = finder.FindGame(errors);
                if (path == null)
                {
                    continue;
                }
                
                errors?.Clear();
                return path;
            }

            return null;
        }
    }
}

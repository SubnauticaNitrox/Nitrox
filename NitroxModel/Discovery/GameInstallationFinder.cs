using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery.InstallationFinders;
using NitroxModel.Helper;

namespace NitroxModel.Discovery
{
    /// <summary>
    ///     Main game installation finder that will use all available methods of detection to find the Subnautica installation
    ///     directory.
    /// </summary>
    public class GameInstallationFinder : IFindGameInstallation
    {
        private readonly IFindGameInstallation[] finders = {
            new ConfigFileGameFinder(),
            new SteamGameRegistryFinder(),
            new EpicGamesRegistryFinder()
        };
        
        /// <summary>
        /// Thread safe backing field for Singleton instance.
        /// </summary>
        private static readonly Lazy<GameInstallationFinder> instance = new Lazy<GameInstallationFinder>(() => new GameInstallationFinder());
        public static GameInstallationFinder Instance => instance.Value;

        public Optional<string> FindGame(List<string> errors = null)
        {
            foreach (IFindGameInstallation finder in finders)
            {
                Optional<string> path = finder.FindGame(errors);
                if (!path.IsPresent())
                {
                    continue;
                }
                
                errors?.Clear();
                return path;
            }

            return Optional<string>.Empty();
        }
    }
}

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
        private readonly List<IFindGameInstallation> finders = new List<IFindGameInstallation>
        {

            new ConfigFileGameFinder()
        };

        public static GameInstallationFinder Instance { get; } = new GameInstallationFinder();

        public Optional<string> FindGame(List<string> errors)
        {
            Validate.NotNull(errors);

            foreach (IFindGameInstallation finder in finders)
            {
                Optional<string> path = finder.FindGame(errors);
                if (path.IsPresent())
                {
                    errors.Clear();
                    return path;
                }
            }

            return Optional<string>.Empty();
        }
    }
}

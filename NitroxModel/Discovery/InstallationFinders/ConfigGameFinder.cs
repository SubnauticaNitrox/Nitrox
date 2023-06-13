using System.Collections.Generic;
using System.IO;
using NitroxModel.Helper;

namespace NitroxModel.Discovery.InstallationFinders
{
    /// <summary>
    ///     Tries to read a local config value that contains the installation directory of Subnautica.
    /// </summary>
    public class ConfigGameFinder : IFindGameInstallation
    {
        public string FindGame(IList<string> errors = null)
        {
            string path = NitroxUser.PreferredGamePath;
#if SUBNAUTICA
            if (string.IsNullOrEmpty(path))
            {
                errors?.Add(@"Configured game path was found empty. Please enter the path to the Subnautica installation.");
                return null;
            }

            if (!Directory.Exists(Path.Combine(path, "Subnautica_Data", "Managed")))
            {
                errors?.Add($@"Game installation directory config '{path}' is invalid. Please enter the path to the Subnautica installation.");
                return null;
            }
#endif
#if BELOWZERO
            if (string.IsNullOrEmpty(path))
            {
                errors?.Add(@"Configured game path was found empty. Please enter the path to the Subnautica Below Zero installation.");
                return null;
            }

            if (!Directory.Exists(Path.Combine(path, "SubnauticaZero_Data", "Managed")))
            {
                errors?.Add($@"Game installation directory config '{path}' is invalid. Please enter the path to the Subnautica Below Zero installation.");
                return null;
            }
#endif

            return path;
        }
    }
}

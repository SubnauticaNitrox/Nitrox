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

            return path;
        }
    }
}

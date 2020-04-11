using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Discovery.InstallationFinders
{
    /// <summary>
    ///     Tries to read a local file that contains the installation directory of Subnautica.
    /// </summary>
    public class ConfigFileGameFinder : IFindGameInstallation
    {
        public delegate string ConfigResolver();

        private static event ConfigResolver getConfig;

        public static void AddResolver(ConfigResolver resolver)
        {
            getConfig += resolver;
        }

        public Optional<string> FindGame(List<string> errors = null)
        {
            if(getConfig == null)
            {
                // should only happen outside of the launcher
                errors?.Add("Game installation config resolver is not set.");
                return Optional.Empty;
            }
            string config = getConfig.Invoke();

            if (config == "" || config == "False")
            {
                errors?.Add($@"Game installation not set in the launcher settings. Please set it using the launcher.");
                return Optional.Empty;
            }

            string path = config.Trim();

            if (!Directory.Exists(Path.Combine(path, "Subnautica_Data", "Managed")))
            {
                errors?.Add($@"Game installation directory specified in the launcher config is invalid. Please enter the path to the Subnautica installation.");
                return Optional.Empty;
            }

            return Optional.Of(path);
        }
    }
}

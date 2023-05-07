using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
///     Trying to find the path in environment variables by the key SUBNAUTICA_INSTALLATION_PATH that contains the installation directory of Subnautica.
/// </summary>
public class EnvironmentGameFinder : IFindGameInstallation
{
    public string FindGame(IList<string> errors = null)
    {
#if SUBNAUTICA
        string path = Environment.GetEnvironmentVariable("SUBNAUTICA_INSTALLATION_PATH");
        if (string.IsNullOrEmpty(path))
        {
            errors?.Add(@"Configured game path with environment variable SUBNAUTICA_INSTALLATION_PATH was found empty.");
            return null;
        }

        if (!Directory.Exists(Path.Combine(path, "Subnautica_Data", "Managed")))
        {
            errors?.Add($@"Game installation directory config '{path}' is invalid. Please enter the path to the Subnautica installation.");
            return null;
        }
#endif
#if BELOWZERO
        string path = Environment.GetEnvironmentVariable("SUBNAUTICAZERO_INSTALLATION_PATH");
        if (string.IsNullOrEmpty(path))
        {
            errors?.Add(@"Configured game path with environment variable SUBNAUTICAZERO_INSTALLATION_PATH was found empty.");
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

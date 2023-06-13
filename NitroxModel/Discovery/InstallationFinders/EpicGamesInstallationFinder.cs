using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class EpicGamesInstallationFinder : IFindGameInstallation
    {
        private readonly Regex installLocationRegex = new("\"InstallLocation\"[^\"]*\"(.*)\"");

        public string FindGame(IList<string> errors = null)
        {
            string epicGamesManifestsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Epic\EpicGamesLauncher\Data\Manifests");
            if (!Directory.Exists(epicGamesManifestsDir))
            {
                errors?.Add("Epic games manifest directory does not exist. Verify that Epic Games Store has been installed.");
                return null;
            }

            string[] files = Directory.GetFiles(epicGamesManifestsDir, "*.item");
            foreach (string file in files)
            {
                string fileText = File.ReadAllText(file);
                Match match = installLocationRegex.Match(fileText);
#if SUBNAUTICA
                if (match.Success && match.Value.Contains("Subnautica") && !match.Value.Contains("Below"))
#elif BELOWZERO
                if (match.Success && match.Value.Contains("Subnautica") && match.Value.Contains("Below"))
#endif
                {
#if SUBNAUTICA
                    Log.Debug($"Found Subnautica install path in '{Path.GetFullPath(file)}'. Full pattern match: '{match.Value}'");
#elif BELOWZERO
                    Log.Debug($"Found Subnautica Below Zero install path in '{Path.GetFullPath(file)}'. Full pattern match: '{match.Value}'");
#endif
                    return match.Groups[1].Value;
                }
            }

#if SUBNAUTICA
            errors?.Add("Could not find Subnautica installation directory from Epic Games installation records. Verify that Subnautica has been installed with Epic Games Store.");
#elif BELOWZERO
            errors?.Add("Could not find Subnautica Below Zero installation directory from Epic Games installation records. Verify that Subnautica Below Zero has been installed with Epic Games Store.");
#endif
            return null;
        }
    }
}

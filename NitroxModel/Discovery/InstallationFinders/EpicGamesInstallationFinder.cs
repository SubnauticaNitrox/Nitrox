using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NitroxModel.Logger;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class EpicGamesInstallationFinder : IFindGameInstallation
    {
        private readonly Regex installLocationRegex = new Regex("\"InstallLocation\"[^\"]*\"(.*)\"");

        public string FindGame(List<string> errors = null)
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
                if (match.Success && match.Value.Contains("Subnautica") && !match.Value.Contains("Below"))
                {
                    Log.Debug($"Found Subnautica install path in '{Path.GetFullPath(file)}'. Full pattern match: '{match.Value}'");
                    return match.Groups[1].Value;
                }
            }

            errors?.Add("Could not find Subnautica installation directory from Epic Games installation records. Verify that Subnautica has been installed with Epic Games Store.");
            return null;
        }
    }
}

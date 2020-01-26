using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Discovery.InstallationFinders
{
    public class EpicGamesInstallationFinder : IFindGameInstallation
    {
        private Regex installLocationRegex = new Regex("\"InstallLocation\"[^\"]*\"(.*)\",");

        public Optional<string> FindGame(List<string> errors = null)
        {
            string epicGamesManifestsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Epic\EpicGamesLauncher\Data\Manifests");

            if (Directory.Exists(epicGamesManifestsDir))
            {
                string[] files = Directory.GetFiles(epicGamesManifestsDir, "*.item");
                foreach (string file in files)
                {
                    string fileText = File.ReadAllText(file);
                    Match match = installLocationRegex.Match(fileText);
                    if (fileText.Contains("Subnautica") && match.Success)
                    {
                        Console.WriteLine(match.Groups[1].Value);
                        return Optional<string>.Of(match.Groups[1].Value);
                    }
                }
            }

            return Optional<string>.Empty();
        }
    }
}

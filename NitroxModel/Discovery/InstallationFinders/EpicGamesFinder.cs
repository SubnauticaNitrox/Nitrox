using System;
using System.IO;
using System.Text.RegularExpressions;
using NitroxModel.Discovery.InstallationFinders.Core;
using static NitroxModel.Discovery.InstallationFinders.Core.GameFinderResult;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
/// Trying to find the path in the Epic Games installation records.
/// </summary>
public sealed class EpicGamesFinder : IGameFinder
{
    private static readonly Regex installLocationRegex = new("\"InstallLocation\"[^\"]*\"(.*)\"");

    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string commonAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string epicGamesManifestsDir = Path.Combine(commonAppFolder, "Epic", "EpicGamesLauncher", "Data", "Manifests");

        if (!Directory.Exists(epicGamesManifestsDir))
        {
            return Error("Epic games manifest directory does not exist. Verify that Epic Games Store has been installed");
        }

        string[] files = Directory.GetFiles(epicGamesManifestsDir, "*.item");
        foreach (string file in files)
        {
            string fileText = File.ReadAllText(file);
            Match match = installLocationRegex.Match(fileText);

            if (match.Success && match.Value.Contains(gameInfo.Name))
            {
                string matchedPath = Path.GetFullPath(match.Groups[1].Value);
                if (!GameInstallationHelper.HasValidGameFolder(matchedPath, gameInfo))
                {
                    continue;
                }

                return Ok(matchedPath);
            }
        }

        return Error("Could not find game installation directory from Epic Games installation records. Verify that game has been installed with Epic Games Store");
    }
}

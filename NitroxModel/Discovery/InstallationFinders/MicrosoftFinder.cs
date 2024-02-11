using NitroxModel.Discovery.InstallationFinders.Core;
using NitroxModel.Discovery.Models;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
/// MS Store games are stored under <c>C:\XboxGames\[GAME]\Content\</c> by default.
/// It's likely we could read the choosen path from <c>C:\Program Files\WindowsApps</c> but we're unable to read store settings from those folders.
/// </summary>
public sealed class MicrosoftFinder : IGameFinder
{
    public GameInstallation FindGame(GameInfo gameInfo, List<string> errors)
    {
        string path = Path.Combine("C:", "XboxGames", gameInfo.Name, "Content");
        if (!GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            errors.Add($"Game installation directory '{path}' is invalid. Please enter the path to the '{gameInfo.Name}' installation");
            return null;
        }

        return new()
        {
            Path = path,
            GameInfo = gameInfo,
            Origin = GameLibraries.MICROSOFT
        };
    }
}

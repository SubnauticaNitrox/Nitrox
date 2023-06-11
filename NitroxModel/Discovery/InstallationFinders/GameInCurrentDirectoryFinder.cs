using NitroxModel.Discovery.Models;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

public class GameInCurrentDirectoryFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        if (GameInstallationFinder.IsSubnauticaDirectory(currentDirectory, gameInfo))
        {
            return new()
            {
                Path = currentDirectory,
                GameInfo = gameInfo,
                Origin = GameLibraries.CURRENT_DIRECTORY
            };
        }

        return null;
    }
}

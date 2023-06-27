using NitroxModel.Discovery.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxModel.Discovery.InstallationFinders;

public class DiscordGameFinder : IGameFinder
{
    /// <summary>
    ///     Discord games are either in appdata or in C:. So for now we just check these 2 paths until we have a better way.
    ///     Discord stores game files in a subfolder called "content" while the parent folder is used to store Discord related files instead.
    /// </summary>
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        string localAppdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string path = Path.Combine(localAppdataDirectory, "DiscordGames", gameInfo.Name, "content");
        if (GameInstallationFinder.HasGameExecutable(path, gameInfo))
        {
            return new()
            {
                Path = path,
                GameInfo = gameInfo,
                Origin = GameLibraries.DISCORD
            };
        }

        path = Path.Combine("C:", "Games", gameInfo.Name, "content");
        if (GameInstallationFinder.HasGameExecutable(path, gameInfo))
        {
            return new()
            {
                Path = path,
                GameInfo = gameInfo,
                Origin = GameLibraries.DISCORD
            };
        }

        return null;
    }
}

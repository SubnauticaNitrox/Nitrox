using System;
using System.IO;
using NitroxModel.Discovery.InstallationFinders.Core;
using static NitroxModel.Discovery.InstallationFinders.Core.GameFinderResult;

namespace NitroxModel.Discovery.InstallationFinders;

/// <summary>
/// Trying to find install either in appdata or in C:. So for now we just check these 2 paths until we have a better way.
/// Discord stores game files in a subfolder called "content" while the parent folder is used to store Discord related files instead.
/// </summary>
public sealed class DiscordFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string localAppdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string path = Path.Combine(localAppdataDirectory, "DiscordGames", gameInfo.Name, "content");
        if (GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            return Ok(path);
        }

        path = Path.Combine("C:", "Games", gameInfo.Name, "content");
        if (GameInstallationHelper.HasValidGameFolder(path, gameInfo))
        {
            return Ok(path);
        }

        return NotFound();
    }
}

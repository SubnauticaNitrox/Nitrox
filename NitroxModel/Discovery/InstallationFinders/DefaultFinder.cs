using NitroxModel.Discovery.Models;
using System.Collections.Generic;

namespace NitroxModel.Discovery.InstallationFinders;

public class DefaultFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        return null;
    }
}

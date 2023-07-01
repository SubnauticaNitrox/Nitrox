using NitroxModel.Discovery.Models;
using System.Collections.Generic;

namespace NitroxModel.Discovery.InstallationFinders;

public class NullGameFinder : IGameFinder
{
    public GameInstallation? FindGame(GameInfo gameInfo, IList<string> errors)
    {
        return null;
    }
}

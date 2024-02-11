using System;

namespace NitroxModel.Discovery.Models;

[Serializable]
public sealed class GameInstallation
{
    public string Path { get; init; }

    public GameInfo GameInfo { get; init; }

    public GameLibraries Origin { get; init; }

    public GameInstallation()
    {

    }

    public GameInstallation(string path, GameInfo gameInfo, GameLibraries origin)
    {
        Path = path;
        GameInfo = gameInfo;
        Origin = origin;
    }

    public override string ToString()
    {
        return $"[Path: '{Path}', Game: '{GameInfo.Name}', Origin: '{Origin}']";
    }
}

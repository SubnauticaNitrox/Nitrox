namespace NitroxModel.Discovery.Models;

public readonly record struct GameInstallation(
    string Path,
    GameInfo GameInfo,
    GameLibraries Origin
);

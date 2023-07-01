using System;

namespace NitroxModel.Discovery.Models;

public readonly record struct GameInstallation(
    string Path,
    GameInfo GameInfo,
    GameLibraries Origin
)
{
    public Platform OriginPlatform => Enum.IsDefined(typeof(Platform), (int)Origin) ? (Platform)Origin : Platform.NONE;
}

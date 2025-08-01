using NitroxModel.Discovery.Models;

namespace Nitrox.Launcher.Models.Design;

public class KnownGame
{
    public required string PathToGame { get; init; }
    public required Platform Platform { get; init; }
}

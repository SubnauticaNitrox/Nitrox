using ReactiveUI;

namespace Nitrox.Launcher.Models.Design;

public class RoutingScreen : IScreen
{
    public RoutingState Router { get; } = new();
}

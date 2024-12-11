using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Design;

public interface IRoutingScreen
{
    public RoutableViewModelBase ActiveViewModel { get; set; }
}

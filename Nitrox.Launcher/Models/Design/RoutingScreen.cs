using CommunityToolkit.Mvvm.ComponentModel;

namespace Nitrox.Launcher.Models.Design;

public partial class RoutingScreen : ObservableObject, IRoutingScreen
{
    [ObservableProperty]
    private object activeViewModel;
}

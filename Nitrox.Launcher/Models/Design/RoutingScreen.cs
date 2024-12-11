using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Design;

public partial class RoutingScreen : ObservableObject, IRoutingScreen
{
    [ObservableProperty]
    private RoutableViewModelBase activeViewModel;
}

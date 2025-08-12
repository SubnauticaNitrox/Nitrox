using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Nitrox.Launcher.Models.Design;

public partial class RoutingScreen : ObservableObject, IRoutingScreen
{
    [ObservableProperty]
    private object activeViewModel;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActiveViewModel))
        {
            WeakReferenceMessenger.Default.Send(new ViewShownMessage(ActiveViewModel));
        }
        base.OnPropertyChanged(e);
    }
}

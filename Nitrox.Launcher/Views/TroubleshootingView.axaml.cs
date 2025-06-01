using Avalonia.Interactivity;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Views;

public partial class TroubleshootingView : RoutableViewBase<TroubleshootingViewModel>
{
    public TroubleshootingView()
    {
        InitializeComponent();
    }

    public void EnablePortChecking(object? sender, RoutedEventArgs e)
    {
        
    }

    public void DisablePortChecking(object? sender, RoutedEventArgs e)
    {
        
    }
}

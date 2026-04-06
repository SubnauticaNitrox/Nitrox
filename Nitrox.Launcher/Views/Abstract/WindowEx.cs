using Avalonia.Controls;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

internal abstract class WindowEx<T> : Window where T : ViewModelBase
{
    protected override void OnInitialized()
    {
        this.ApplyOsWindowStyling();
        this.ApplyPlatformWindowChrome();
    }
}

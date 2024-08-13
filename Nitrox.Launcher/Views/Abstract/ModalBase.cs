using Avalonia.Controls;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class ModalBase : Window
{
    protected ModalBase()
    {
        SystemDecorations = SystemDecorations.Full;
    }

    protected override void OnInitialized() => this.ApplyOsWindowStyling();
}

using Avalonia.Controls;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class ModalBase : Window
{
    protected ModalBase()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}

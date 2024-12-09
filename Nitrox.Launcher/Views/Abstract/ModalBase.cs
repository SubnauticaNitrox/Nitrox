using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class ModalBase : Window
{
    protected ModalBase()
    {
        SystemDecorations = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? SystemDecorations.Full : SystemDecorations.None;
    }

    protected override void OnInitialized() => this.ApplyOsWindowStyling();
}

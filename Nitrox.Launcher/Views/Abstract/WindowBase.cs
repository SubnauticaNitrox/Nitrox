using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class WindowBase<TViewModal> : ReactiveWindow<TViewModal> where TViewModal : class
{
    protected WindowBase()
    {
        SystemDecorations = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? SystemDecorations.Full : SystemDecorations.None;
    }
    
    protected override void OnInitialized() => this.ApplyOsWindowStyling();
}

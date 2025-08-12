using System;
using Avalonia.Controls;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

public abstract class WindowEx<T> : Window where T : ViewModelBase
{
    protected override void OnInitialized()
    {
        this.ApplyOsWindowStyling();

        // On Linux systems, Avalonia has trouble allowing windows to resize without "decorations". So we enable it in full, but hide the custom titlebar as it'll look bad.
        if (OperatingSystem.IsLinux())
        {
            SystemDecorations = SystemDecorations.Full;
            NitroxAttached.SetUseCustomTitleBar(this, false);
        }
    }
}

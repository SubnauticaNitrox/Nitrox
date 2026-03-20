using System;
using Avalonia.Controls;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Views.Abstract;

internal abstract class WindowEx<T> : Window where T : ViewModelBase
{
    protected override void OnInitialized()
    {
        this.ApplyOsWindowStyling();

        // On Linux systems, Avalonia has trouble allowing windows to resize without "decorations". So we enable it in full, but hide the custom titlebar as it'll look bad.
        // On macOS, we need the native toolbar as every app is using it
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            SystemDecorations = SystemDecorations.Full;
            NitroxAttached.SetUseCustomTitleBar(this, false);
        }
    }
}

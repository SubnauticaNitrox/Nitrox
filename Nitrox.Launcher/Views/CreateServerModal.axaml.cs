using System;
using Avalonia.Controls;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Platforms.OS.Windows;

namespace Nitrox.Launcher.Views;

public partial class CreateServerModal : ModalBase
{
    public CreateServerModal()
    {
        InitializeComponent();
        
        // Restore default window animations for Windows OS
        if (!Design.IsDesignMode)
        {
            IntPtr? windowHandle = GetTopLevel(this)?.TryGetPlatformHandle()?.Handle;
            if (windowHandle.HasValue)
            {
                WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value, CanResize);
            }
        }
    }
}

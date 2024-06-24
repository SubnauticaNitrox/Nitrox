using System;
using Avalonia.Controls;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Platforms.OS.Windows;

namespace Nitrox.Launcher.Views;

public partial class DialogBoxModal : ModalBase
{
    public DialogBoxModal()
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

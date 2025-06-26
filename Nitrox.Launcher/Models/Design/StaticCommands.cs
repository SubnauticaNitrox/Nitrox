using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Design;

internal static class StaticCommands
{
    private static AsyncRelayCommand<object>? copyToClipboardCommand;
    public static IAsyncRelayCommand<object> CopyToClipboardCommand => copyToClipboardCommand ??= new AsyncRelayCommand<object>(CopyToClipboard);

    private static async Task CopyToClipboard(object? obj)
    {
        try
        {
            if (obj is not string text)
            {
                text = obj?.ToString();
            }
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            Window window = ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.Windows.FirstOrDefault();
            if (window == null)
            {
                return;
            }
            IClipboard clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            await clipboard.SetTextAsync(text);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error trying to set clipboard");
        }
    }
}

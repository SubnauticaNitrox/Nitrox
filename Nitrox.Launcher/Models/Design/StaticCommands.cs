using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using Nitrox.Model.Logger;

namespace Nitrox.Launcher.Models.Design;

internal static class StaticCommands
{
    public static IAsyncRelayCommand<object> CopyToClipboardCommand => new AsyncRelayCommand<object>(CopyToClipboard);

    private static async Task CopyToClipboard(object? controlOrText)
    {
        try
        {
            if (GetWindowOfObject(controlOrText) is not { } window)
            {
                return;
            }
            string text = controlOrText switch
            {
                ContentControl when window is ModalBase { DataContext: DialogBoxViewModel context } =>
                    $"{context.Title}{Environment.NewLine}{(context.Description.StartsWith(context.Title) ? context.Description[context.Title.Length..].TrimStart() : context.Description)}",
                ContentControl when window is { DataContext: CrashWindowViewModel context } => context.Message,
                ContentControl cc => cc.Content?.ToString(),
                _ => controlOrText?.ToString()
            };
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            IClipboard clipboard = window.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            await clipboard.SetTextAsync(text);
            if (controlOrText is ContentControl control)
            {
                object previousContent = control.Content;
                control.Content = "Copied!";
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await Task.Delay(3000);
                    control.Content = previousContent;
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error trying to set clipboard");
        }

        static Window? GetWindowOfObject(object? obj) =>
            obj switch
            {
                Visual v => v.GetWindow(),
                _ => ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.Windows.FirstOrDefault()
            };
    }
}

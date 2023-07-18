using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

public partial class ErrorViewModel : ModalViewModelBase
{
    public Exception Exception { get; init; }
    public string ErrorTitle => GetTitleFromException(Exception);
    public string ErrorText => Exception.ToString();
    public KeyGesture OkHotkey { get; } = new(Key.Return);
    public KeyGesture CopyToClipboardHotkey { get; } = new(Key.C, KeyModifiers.Control);

    public ErrorViewModel(Exception exception)
    {
        Exception = exception;
    }

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (!string.IsNullOrWhiteSpace(ErrorText))
        {
            IClipboard clipboard = TopLevel.GetTopLevel(AppViewLocator.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(ErrorText);
            }
        }
    }

    [RelayCommand]
    private void Ok(Window window)
    {
        DialogResult = true;
        Close(window);
        Environment.Exit(1);
    }

    private string GetTitleFromException(in Exception exception)
    {
        string title = exception switch
                       {
                           TargetInvocationException e => e.InnerException?.Message,
                           _ => exception.Message
                       } ??
                       exception.Message;
        return $"Error: {title}";
    }
}

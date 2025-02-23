using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Simple Yes/No or OK confirmation box.
/// </summary>
public partial class DialogBoxViewModel : ModalViewModelBase
{
    [ObservableProperty] private string windowTitle;

    [ObservableProperty] private string title;
    [ObservableProperty] private double titleFontSize = 24;
    [ObservableProperty] private FontWeight titleFontWeight = FontWeight.Bold;

    [ObservableProperty] private string description;
    [ObservableProperty] private double descriptionFontSize = 14;
    [ObservableProperty] private FontWeight descriptionFontWeight = FontWeight.Normal;

    [ObservableProperty] private ButtonOptions buttonOptions = ButtonOptions.Ok;

    public KeyGesture OkHotkey { get; } = new(Key.Return);
    public KeyGesture NoHotkey { get; } = new(Key.Escape);
    public KeyGesture CopyToClipboardHotkey { get; } = new(Key.C, KeyModifiers.Control);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Title):
            case nameof(Description):
                if (WindowTitle is null or "")
                {
                    WindowTitle = string.IsNullOrEmpty(Title) ? WindowTitle : Title;
                }
                if (WindowTitle is null or "" && Description is not (null or ""))
                {
                    WindowTitle = $"{Description[..Math.Min(30, Description.Length)]}...";
                }
                break;
        }

        base.OnPropertyChanged(e);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CopyToClipboard(ContentControl commandControl)
    {
        string text = $"{Title}{Environment.NewLine}{(Description.StartsWith(Title) ? Description[Title.Length..].TrimStart() : Description)}";
        IClipboard clipboard = commandControl.GetWindow().Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);

            if (commandControl != null)
            {
                object previousContent = commandControl.Content;
                commandControl.Content = "Copied!";
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await Task.Delay(3000);
                    commandControl.Content = previousContent;
                });
            }
        }
    }
}

[Flags]
public enum ButtonOptions
{
    Ok = 1 << 0,
    Yes = 1 << 1,
    No = 1 << 2,
    Clipboard = 1 << 3,
    OkClipboard = Ok | Clipboard,
    YesNo = Yes | No,
}

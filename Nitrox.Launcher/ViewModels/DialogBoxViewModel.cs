using System;
using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Simple Yes/No or OK confirmation box.
/// </summary>
public partial class DialogBoxViewModel : ModalViewModelBase
{
    [ObservableProperty]
    public partial string? WindowTitle { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial double TitleFontSize { get; set; } = 24;

    [ObservableProperty]
    public partial FontWeight TitleFontWeight { get; set; } = FontWeight.Bold;

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial double DescriptionFontSize { get; set; } = 14;

    [ObservableProperty]
    public partial FontWeight DescriptionFontWeight { get; set; } = FontWeight.Normal;

    [ObservableProperty]
    public partial ButtonOptions ButtonOptions { get; set; } = ButtonOptions.Ok;

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

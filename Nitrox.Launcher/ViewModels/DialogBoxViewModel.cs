using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Simple yes/no or OK confirmation box. The yes button doesn't have a hotkey to prevent accidental confirmation.
/// </summary>
public partial class DialogBoxViewModel : ModalViewModelBase
{
    [ObservableProperty] private string windowTitle;

    [ObservableProperty] private string title;
    [ObservableProperty] private IBrush titleForeground = Brushes.Black;
    [ObservableProperty] private double titleFontSize = 24;
    [ObservableProperty] private FontWeight titleFontWeight = FontWeight.Bold;

    [ObservableProperty] private string description;
    [ObservableProperty] private IBrush descriptionForeground = Brushes.Black;
    [ObservableProperty] private double descriptionFontSize = 14;
    [ObservableProperty] private FontWeight descriptionFontWeight = FontWeight.Normal;

    [ObservableProperty] private ButtonOptions buttonOptions = ButtonOptions.Ok;

    public KeyGesture OkHotkey { get; } = new(Key.Return);
    public KeyGesture NoHotkey { get; } = new(Key.Escape);
    public KeyGesture CopyToClipboardHotkey { get; } = new(Key.C, KeyModifiers.Control);

    public DialogBoxViewModel()
    {
        this.WhenAnyValue(model => model.Title, model => model.Description)
            .Subscribe(tuple =>
            {
                (string titleText, string descriptionText) = tuple;
                WindowTitle ??= string.IsNullOrEmpty(titleText) ? WindowTitle : titleText;
                WindowTitle ??= string.IsNullOrEmpty(descriptionText) ? WindowTitle : $"{descriptionText[..Math.Min(30, descriptionText.Length)]}...";
            })
            .DisposeWith(Disposables);
    }

    [RelayCommand]
    private void OptionSelect(ButtonOptions option)
    {
        SelectedOption = option;
        Close();
    }

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        string text = $"{Title}{Environment.NewLine}{(Description.StartsWith(Title) ? Description[Title.Length..].TrimStart() : Description)}";
        IClipboard clipboard = TopLevel.GetTopLevel(AppViewLocator.MainWindow)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
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

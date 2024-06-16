using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Simple yes/no or OK confirmation box. The yes button doesn't have a hotkey to prevent accidental confirmation.
/// </summary>
public partial class DialogBoxViewModal : ModalViewModelBase
{
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

    [RelayCommand]
    private void Yes(Window window)
    {
        DialogResult = true;
        Close(window);
    }
    
    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            IClipboard clipboard = TopLevel.GetTopLevel(AppViewLocator.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(Description);
            }
        }
    }
}

public enum ButtonOptions
{
    Ok,
    OkClipboard,
    YesNo,
}

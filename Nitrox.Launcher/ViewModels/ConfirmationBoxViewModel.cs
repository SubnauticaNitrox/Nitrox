using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Simple yes/no confirmation box. The yes button doesn't have a hotkey to prevent accidental confirmation.
/// </summary>
public partial class ConfirmationBoxViewModel : ModalViewModelBase
{
    [ObservableProperty] private string confirmationText;

    public KeyGesture NoHotkey { get; } = new(Key.Escape);

    [RelayCommand]
    private void Yes(Window window)
    {
        DialogResult = true;
        Close(window);
    }
}

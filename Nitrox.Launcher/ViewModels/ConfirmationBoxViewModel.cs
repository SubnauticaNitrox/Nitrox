using System.ComponentModel.DataAnnotations;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.ViewModels;

public partial class ConfirmationBoxViewModel : ModalViewModelBase
{
    [ObservableProperty]
    private string confirmationText;
    
    public KeyGesture NoHotkey { get; } = new(Key.Escape);
    public KeyGesture YesHotkey { get; } = new(Key.Return);

    public ConfirmationBoxViewModel(string confirmationBoxText)
    {
        ConfirmationText = confirmationBoxText ?? "Are you sure you want to delete this server?"; // Null check replacement message is temp
    }

    [RelayCommand]
    private void Yes(Window window)
    {
        DialogResult = true;
        Close(window);
    }

}

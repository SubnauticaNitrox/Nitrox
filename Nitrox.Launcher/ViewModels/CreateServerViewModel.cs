using System.ComponentModel.DataAnnotations;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Server;

namespace Nitrox.Launcher.ViewModels;

public partial class CreateServerViewModel : ModalViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(NitroxValidation), nameof(NitroxValidation.IsValidFileName))]
    private string name;
    [ObservableProperty]
    private ServerGameMode selectedGameMode = ServerGameMode.SURVIVAL;
    public KeyGesture BackHotkey { get; } = new(Key.Escape);
    public KeyGesture CreateHotkey { get; } = new(Key.Return);

    public CreateServerViewModel()
    {
        // At least one property has an "invalid" default value, so we need to trigger it manually so that the Create button is disabled.
        ValidateAllProperties();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create(Window window)
    {
        DialogResult = true;
        Close(window);
    }

    // TODO: Validate that the name isn't a duplicate of another save
    private bool CanCreate() => !HasErrors;
}

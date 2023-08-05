using System.ComponentModel.DataAnnotations;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Server;
using NitroxServer.Serialization.World;

namespace Nitrox.Launcher.ViewModels;

public partial class CreateServerViewModel : ModalViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NitroxUniqueSaveName]
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
        WorldManager.CreateEmptySave(Path.Combine(WorldManager.SavesFolderDir, Name.Trim()));
        Close(window);
    }
    
    private bool CanCreate() => !HasErrors;
}

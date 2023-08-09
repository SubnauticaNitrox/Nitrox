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
        CreateEmptySave(Name, SelectedGameMode);
        Close(window);
    }

    public static void CreateEmptySave(string saveName, ServerGameMode saveGameMode)
    {
        string saveDir = Path.Combine(ServersViewModel.SavesFolderDir, saveName);
        Directory.CreateDirectory(saveDir);
        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = "json";
        if (config.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = "nitrox";
        }
        File.Create(Path.Combine(saveDir, $"Version.{fileEnding}")).DisposeAsync();
        
        using (config.Update(saveDir))
        {
            config.SaveName = saveName;
            config.GameMode = saveGameMode;
        }
    }

    private bool CanCreate() => !HasErrors;
}

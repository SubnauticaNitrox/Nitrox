using System.ComponentModel.DataAnnotations;
using System.IO;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.ViewModels;

public partial class CreateServerViewModel : ModalViewModelBase
{
    private readonly IKeyValueStore keyValueStore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NotEndsWith(".")]
    [NitroxUniqueSaveName(nameof(SavesFolderDir))]
    private string name;

    [ObservableProperty]
    private NitroxGameMode selectedGameMode = NitroxGameMode.SURVIVAL;

    public KeyGesture BackHotkey { get; } = new(Key.Escape);
    public KeyGesture CreateHotkey { get; } = new(Key.Return);

    private string SavesFolderDir => keyValueStore.GetSavesFolderDir();

    public CreateServerViewModel(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    public void CreateEmptySave(string saveName, NitroxGameMode saveGameMode)
    {
        string saveDir = Path.Combine(SavesFolderDir, saveName);
        Directory.CreateDirectory(saveDir);
        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = "json";
        if (config.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = "nitrox";
        }

        File.Create(Path.Combine(saveDir, $"Version.{fileEnding}")).Dispose();

        using (config.Update(saveDir))
        {
            config.SaveName = saveName;
            config.GameMode = saveGameMode;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        CreateEmptySave(Name, SelectedGameMode);
        Close();
    }

    private bool CanCreate() => !HasErrors;
}

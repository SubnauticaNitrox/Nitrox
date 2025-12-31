using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;

namespace Nitrox.Launcher.ViewModels;

public partial class CreateServerViewModel : ModalViewModelBase
{
    private readonly IKeyValueStore? keyValueStore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NotEndsWith(".")]
    [NitroxUniqueSaveName(nameof(SavesFolderDir))]
    private string name = "";

    [ObservableProperty]
    private SubnauticaGameMode selectedGameMode = SubnauticaGameMode.SURVIVAL;

    private string SavesFolderDir => keyValueStore.GetSavesFolderDir();

    public CreateServerViewModel()
    {
    }

    public CreateServerViewModel(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    public async Task CreateEmptySave(string saveName, SubnauticaGameMode saveGameMode)
    {
        string saveDir = Path.Combine(SavesFolderDir, saveName);
        await ServerEntry.CreateNew(saveDir, saveGameMode);
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        await Task.Run(async () => await CreateEmptySave(Name, SelectedGameMode));
        Close(ButtonOptions.Ok);
    }

    private bool CanCreate() => !HasErrors;
}

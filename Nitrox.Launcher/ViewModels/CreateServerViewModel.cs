using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
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

    private string SavesFolderDir => keyValueStore.GetSavesFolderDir();

    public CreateServerViewModel()
    {
    }

    public CreateServerViewModel(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    public void CreateEmptySave(string saveName, NitroxGameMode saveGameMode)
    {
        string saveDir = Path.Combine(SavesFolderDir, saveName);
        ServerEntry.CreateNew(saveDir, saveGameMode);
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        await Task.Run(() => CreateEmptySave(Name, SelectedGameMode));
        Close(ButtonOptions.Ok);
    }

    private bool CanCreate() => !HasErrors;
}

using System.Reactive;
using Avalonia.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Nitrox.Launcher.ViewModels;

public class CreateServerViewModel : ModalViewModelBase
{

    private string name = "";
    private GameMode selectedGameMode = GameMode.SURVIVAL;
    public ReactiveCommand<Unit, CreateServerViewModel> CreateServerCommand { get; }
    public ReactiveCommand<Unit, CreateServerViewModel> BackCommand { get; }

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public KeyGesture BackHotkey { get; } = new(Key.Escape);
    public KeyGesture CreateHotkey { get; } = new(Key.Return);

    public GameMode SelectedGameMode
    {
        get => selectedGameMode;
        set => this.RaiseAndSetIfChanged(ref selectedGameMode, value);
    }

    public CreateServerViewModel()
    {
        BackCommand = ReactiveCommand.Create(() => (CreateServerViewModel)null!);
        CreateServerCommand = ReactiveCommand.Create(() => this, this.IsValid());

        this.BindValidation();
    }
}

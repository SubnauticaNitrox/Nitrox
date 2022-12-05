using System.Reactive;
using Avalonia.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Nitrox.Launcher.ViewModels;

public class CreateServerViewModel : ModalViewModelBase
{
    private string name = "";
    public ReactiveCommand<Unit, CreateServerViewModel> CreateServerCommand { get; }
    public ReactiveCommand<Unit, CreateServerViewModel> BackCommand { get; }

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value.Trim());
    }

    public KeyGesture BackHotkey { get; } = new(Key.Escape);
    public KeyGesture CreateHotkey { get; } = new(Key.Return);

    public CreateServerViewModel()
    {
        BackCommand = ReactiveCommand.Create(() => (CreateServerViewModel)null!);
        CreateServerCommand = ReactiveCommand.Create(() => this, this.IsValid());
        
        this.ValidationRule(vm => vm.Name, value => !string.IsNullOrWhiteSpace(value), $"{nameof(Name)} shouldn't be empty.");
    }
}

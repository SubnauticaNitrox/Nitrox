using System.Reactive;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class CreateServerViewModel : IModalViewModel
{
    public ReactiveCommand<Unit, CreateServerViewModel> CreateServerCommand { get; }
    public string Name { get; set; } = "";

    public CreateServerViewModel()
    {
        CreateServerCommand = ReactiveCommand.Create(() => this);
    }
}

using System.Reactive;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class CreateServerViewModel
{
    public ReactiveCommand<Unit, CreateServerViewModel> CreateServerCommand { get; }
    public string Name { get; set; }
    
    public CreateServerViewModel()
    {
        CreateServerCommand = ReactiveCommand.Create(() => this);
    }
}

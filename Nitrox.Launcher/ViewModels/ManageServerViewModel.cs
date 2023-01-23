using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class ManageServerViewModel : RoutableViewModelBase
{
    private ServerEntry server;

    public ServerEntry Server
    {
        get => server;
        set => this.RaiseAndSetIfChanged(ref server, value);
    }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }
}

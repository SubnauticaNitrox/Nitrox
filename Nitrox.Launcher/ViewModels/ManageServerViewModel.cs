using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class ManageServerViewModel : RoutableViewModelBase
{
    private ServerEntry server;

    /// <summary>
    ///     When set, navigates to the <see cref="ManageServerView" />.
    /// </summary>
    public ServerEntry Server
    {
        get => server;
        set
        {
            this.RaisePropertyChanging();
            server = value;
            this.RaisePropertyChanged();
        }
    }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }
}

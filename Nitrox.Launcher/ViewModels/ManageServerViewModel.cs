using System.Reactive;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class ManageServerViewModel : RoutableViewModelBase
{
    private string serverName;

    /// <summary>
    ///     When set, navigates to the <see cref="ManageServerView" />.
    /// </summary>
    public string ServerName
    {
        get => serverName;
        set => this.RaiseAndSetIfChanged(ref serverName, value);
    }

    public ReactiveCommand<Unit, IRoutableViewModel> BackCommand { get; init; }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        this.BindValidation();
        BackCommand = Router.NavigateBack;
    }

    public void LoadFrom(ServerEntry server)
    {
        ServerName = server.Name;
    }
}

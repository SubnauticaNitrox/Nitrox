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
    
    private string serverPassword;
    /// <summary>
    ///     When set, navigates to the <see cref="ManageServerView" />.
    /// </summary>
    public string ServerPassword
    {
        get => serverPassword;
        set => this.RaiseAndSetIfChanged(ref serverPassword, value);
    }

    private GameMode serverGameMode;
    public GameMode ServerGameMode
    {
        get => serverGameMode;
        set => this.RaiseAndSetIfChanged(ref serverGameMode, value);
    }
    
    private string serverSeed;
    public string ServerSeed
    {
        get => serverSeed;
        set => this.RaiseAndSetIfChanged(ref serverSeed, value);
    }
    
    private int serverPlayerLimit;
    public int ServerPlayerLimit
    {
        get => serverPlayerLimit;
        set => this.RaiseAndSetIfChanged(ref serverPlayerLimit, value);
    }
    
    private int serverPort;
    public int ServerPort
    {
        get => serverPort;
        set => this.RaiseAndSetIfChanged(ref serverPort, value);
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
        ServerPassword = ""; // Need to add
        ServerGameMode = server.GameMode;
        serverSeed = ""; // Need to add

        serverPlayerLimit = server.MaxPlayers;
        serverPort = 11000; // Need to add
    }
}

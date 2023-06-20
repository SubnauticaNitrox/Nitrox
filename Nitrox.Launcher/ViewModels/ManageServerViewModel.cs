using System.Reactive;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
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
    
    
    private bool serverIsOnline;
    public bool ServerIsOnline
    {
        get => serverIsOnline;
        set => this.RaiseAndSetIfChanged(ref serverIsOnline, value);
    }
    
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
    
    private PlayerPermissions serverDefaultPerms;
    public PlayerPermissions ServerDefaultPerms
    {
        get => serverDefaultPerms;
        set => this.RaiseAndSetIfChanged(ref serverDefaultPerms, value);
    }
    
    private int serverAutoSaveInterval;
    public int ServerAutoSaveInterval
    {
        get => serverAutoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref serverAutoSaveInterval, value);
    }
    
    private int serverPlayerLimit;
    public int ServerPlayerLimit
    {
        get => serverPlayerLimit;
        set => this.RaiseAndSetIfChanged(ref serverPlayerLimit, value);
    }
    
    private int serverPlayerCount;
    public int ServerPlayerCount
    {
        get => serverPlayerCount;
        set => this.RaiseAndSetIfChanged(ref serverPlayerCount, value);
    }
    
    private int serverPort;
    public int ServerPort
    {
        get => serverPort;
        set => this.RaiseAndSetIfChanged(ref serverPort, value);
    }
    
    private bool autoPortForward;
    public bool AutoPortForward
    {
        get => autoPortForward;
        set => this.RaiseAndSetIfChanged(ref autoPortForward, value);
    }
    
    private bool allowLanDiscovery;
    public bool AllowLanDiscovery
    {
        get => allowLanDiscovery;
        set => this.RaiseAndSetIfChanged(ref allowLanDiscovery, value);
    }
    
    private bool enableCommands;
    public bool EnableCommands
    {
        get => enableCommands;
        set => this.RaiseAndSetIfChanged(ref enableCommands, value);
    }

    private string worldFolderDirectory;
    
    public ReactiveCommand<Unit, IRoutableViewModel> BackCommand { get; init; }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        this.BindValidation();
        BackCommand = Router.NavigateBack;
    }

    public void LoadFrom(ServerEntry serverEntry)
    {
        Server = serverEntry;
        worldFolderDirectory = Server.SavePath;

        ServerIsOnline = Server.IsOnline;
        
        ServerName = Server.Name;
        ServerPassword = Server.Password;
        ServerGameMode = Server.GameMode;
        ServerSeed = Server.Seed;
        ServerDefaultPerms = Server.DefaultPlayerPerm;
        ServerAutoSaveInterval = Server.AutoSaveInterval;
        ServerPlayerLimit = Server.MaxPlayers;
        ServerPlayerCount = Server.Players;
        ServerPort = Server.Port;
        AutoPortForward = Server.AutoPortForward;
        AllowLanDiscovery = Server.AllowLanDiscovery;
        EnableCommands = Server.AllowCommands;

    }
}

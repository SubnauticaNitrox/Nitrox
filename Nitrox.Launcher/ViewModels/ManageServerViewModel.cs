using System;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
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
            if (server != null)
            {
                server.PropertyChanged -= Server_PropertyChanged;
            }
            this.RaiseAndSetIfChanged(ref server, value);
            if (server != null)
            {
                server.PropertyChanged += Server_PropertyChanged;
            }
        }
    }
    
    private bool serverIsOnline;
    public bool ServerIsOnline
    {
        get => Server.IsOnline;
        set => this.RaiseAndSetIfChanged(ref serverIsOnline, value);
    }

    private string serverName;
    public string ServerName
    {
        get => serverName;
        set => this.RaiseAndSetIfChanged(ref serverName, value);
    }

    private string serverPassword;
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

    private PlayerPermissions serverDefaultPlayerPerm;
    public PlayerPermissions ServerDefaultPlayerPerm
    {
        get => serverDefaultPlayerPerm;
        set => this.RaiseAndSetIfChanged(ref serverDefaultPlayerPerm, value);
    }

    private int serverAutoSaveInterval;
    public int ServerAutoSaveInterval
    {
        get => serverAutoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref serverAutoSaveInterval, value);
    }

    private int serverMaxPlayers;
    public int ServerMaxPlayers
    {
        get => serverMaxPlayers;
        set => this.RaiseAndSetIfChanged(ref serverMaxPlayers, value);
    }

    private int serverPlayers;
    public int ServerPlayers
    {
        get => serverPlayers;
        set => this.RaiseAndSetIfChanged(ref serverPlayers, value);
    }

    private int serverPort;
    public int ServerPort
    {
        get => serverPort;
        set => this.RaiseAndSetIfChanged(ref serverPort, value);
    }

    private bool serverAutoPortForward;
    public bool ServerAutoPortForward
    {
        get => serverAutoPortForward;
        set => this.RaiseAndSetIfChanged(ref serverAutoPortForward, value);
    }

    private bool serverAllowLanDiscovery;
    public bool ServerAllowLanDiscovery
    {
        get => serverAllowLanDiscovery;
        set => this.RaiseAndSetIfChanged(ref serverAllowLanDiscovery, value);
    }
    
    private bool serverAllowCommands;
    public bool ServerAllowCommands
    {
        get => serverAllowCommands;
        set => this.RaiseAndSetIfChanged(ref serverAllowCommands, value);
    }
    
    private string worldFolderDirectory;
    
    public ReactiveCommand<Unit, Unit> BackCommand { get; init; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; init; }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        this.BindValidation();
        
        BackCommand = ReactiveCommand.Create(() =>
        {
            Save(); // TEMP - Will be replaced by Save button/command (below)
            Router.NavigateBack.Execute();
        });
        
        SaveCommand = ReactiveCommand.Create(() =>
        {
            Server.WhenAnyValue(x => x.IsOnline).Where(x => !x);
            
            Server.Name = ServerName;
            Server.Password = ServerPassword;
            Server.GameMode = ServerGameMode;
            Server.Seed = ServerSeed;
            Server.DefaultPlayerPerm = ServerDefaultPlayerPerm;
            Server.AutoSaveInterval = ServerAutoSaveInterval;
            Server.MaxPlayers = ServerMaxPlayers;
            Server.Players = ServerPlayers;
            Server.Port = ServerPort;
            Server.AutoPortForward = ServerAutoPortForward;
            Server.AllowLanDiscovery = ServerAllowLanDiscovery;
            Server.AllowCommands = ServerAllowCommands;
        });
    }
    public void LoadFrom(ServerEntry serverEntry)
    {
        Server = serverEntry;
        worldFolderDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves", Server.Name);
        
        ServerIsOnline = Server.IsOnline;
        
        ServerName = Server.Name;
        ServerPassword = Server.Password;
        ServerGameMode = Server.GameMode;
        ServerSeed = Server.Seed;
        ServerDefaultPlayerPerm = Server.DefaultPlayerPerm;
        ServerAutoSaveInterval = Server.AutoSaveInterval;
        ServerMaxPlayers = Server.MaxPlayers;
        ServerPlayers = Server.Players;
        ServerPort = Server.Port;
        ServerAutoPortForward = Server.AutoPortForward;
        ServerAllowLanDiscovery = Server.AllowLanDiscovery;
        ServerAllowCommands = Server.AllowCommands;
    }

    private void Save() // TEMP - Will be replaced
    {
        // TODO: Check for invalid value inputs and throw an error if one is found
        
        Server.Name = ServerName;
        Server.Password = ServerPassword;
        Server.GameMode = ServerGameMode;
        Server.Seed = ServerSeed;
        Server.DefaultPlayerPerm = ServerDefaultPlayerPerm;
        Server.AutoSaveInterval = ServerAutoSaveInterval;
        Server.MaxPlayers = ServerMaxPlayers;
        Server.Players = ServerPlayers;
        Server.Port = ServerPort;
        Server.AutoPortForward = ServerAutoPortForward;
        Server.AllowLanDiscovery = ServerAllowLanDiscovery;
        Server.AllowCommands = ServerAllowCommands;
    }
    
    private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerEntry.IsOnline))
        {
            this.RaisePropertyChanged(nameof(ServerIsOnline));
        }
    }
}

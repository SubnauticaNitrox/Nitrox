using System;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
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
            CheckIfAnySettingChanged();
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
        set
        {
            this.RaiseAndSetIfChanged(ref serverName, value);
            CheckIfAnySettingChanged();
        }
    }

    private string serverPassword;
    public string ServerPassword
    {
        get => serverPassword;
        set
        { 
            this.RaiseAndSetIfChanged(ref serverPassword, value);
            CheckIfAnySettingChanged();
        }
    }

    private GameMode serverGameMode;
    public GameMode ServerGameMode
    {
        get => serverGameMode;
        set
        {
            this.RaiseAndSetIfChanged( ref serverGameMode, value);
            CheckIfAnySettingChanged();
        }
    }

    private string serverSeed;
    public string ServerSeed
    {
        get => serverSeed;
        set
        {
            this.RaiseAndSetIfChanged(ref serverSeed, value);
            CheckIfAnySettingChanged();
        }
    }

    private PlayerPermissions serverDefaultPlayerPerm;
    public PlayerPermissions ServerDefaultPlayerPerm
    {
        get => serverDefaultPlayerPerm;
        set
        {
            this.RaiseAndSetIfChanged(ref serverDefaultPlayerPerm, value);
            CheckIfAnySettingChanged();
        }
    }

    private int serverAutoSaveInterval;
    public int ServerAutoSaveInterval
    {
        get => serverAutoSaveInterval;
        set
        {
            this.RaiseAndSetIfChanged(ref serverAutoSaveInterval, value);
            CheckIfAnySettingChanged();
        }
    }

    private int serverMaxPlayers;
    public int ServerMaxPlayers
    {
        get => serverMaxPlayers;
        set
        {
            this.RaiseAndSetIfChanged(ref serverMaxPlayers, value);
            CheckIfAnySettingChanged();
        }
    }

    private int serverPlayers;
    public int ServerPlayers
    {
        get => serverPlayers;
        set
        {
            this.RaiseAndSetIfChanged(ref serverPlayers, value);
            CheckIfAnySettingChanged();
        }
    }

    private int serverPort;
    public int ServerPort
    {
        get => serverPort;
        set
        {
            this.RaiseAndSetIfChanged(ref serverPort, value);
            CheckIfAnySettingChanged();
        }
    }

    private bool serverAutoPortForward;
    public bool ServerAutoPortForward
    {
        get => serverAutoPortForward;
        set
        {
            this.RaiseAndSetIfChanged(ref serverAutoPortForward, value);
            CheckIfAnySettingChanged();
        }
    }

    private bool serverAllowLanDiscovery;
    public bool ServerAllowLanDiscovery
    {
        get => serverAllowLanDiscovery;
        set
        {
            this.RaiseAndSetIfChanged(ref serverAllowLanDiscovery, value);
            CheckIfAnySettingChanged();
        }
    }
    
    private bool serverAllowCommands;
    public bool ServerAllowCommands
    {
        get => serverAllowCommands;
        set
        {
            this.RaiseAndSetIfChanged(ref serverAllowCommands, value);
            CheckIfAnySettingChanged();
        }
    }
    
    private string worldFolderDirectory;

    private bool isAnySettingChanged;
    public bool IsAnySettingChanged
    {
        get => isAnySettingChanged;
        set => this.RaiseAndSetIfChanged(ref isAnySettingChanged, value);
    }

    private bool CheckIfAnySettingChanged()
    {
        if (ServerName != Server.Name || ServerPassword != Server.Password ||
            ServerGameMode != Server.GameMode || ServerSeed != Server.Seed ||
            ServerDefaultPlayerPerm != Server.DefaultPlayerPerm || ServerAutoSaveInterval != Server.AutoSaveInterval ||
            ServerMaxPlayers != Server.MaxPlayers || ServerPlayers != Server.Players ||
            ServerPort != Server.Port || ServerAutoPortForward != Server.AutoPortForward ||
            ServerAllowLanDiscovery != Server.AllowLanDiscovery || ServerAllowCommands != Server.AllowCommands)
        {
            IsAnySettingChanged = true;
        }
        else
        {
            IsAnySettingChanged = false;
        }

        return IsAnySettingChanged;
    }
    
    public ReactiveCommand<Unit, Unit> BackCommand { get; init; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; init; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; init; }
    public ReactiveCommand<Unit, Unit> StartServerCommand { get; init; }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        this.BindValidation();
        
        IObservable<bool> canExecuteManageServerCommands = this.WhenAnyValue(x => x.IsAnySettingChanged);
        
        BackCommand = ReactiveCommand.Create(() =>
        {
            if (CheckIfAnySettingChanged())
            {
                Console.WriteLine("Save your changes before continuing"); // TODO: Launcher Notification
            }
            else
            {
                Router.NavigateBack.Execute();
            }
        });
        
        SaveCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Check for invalid value inputs and throw an error if one is found
            
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
            
            CheckIfAnySettingChanged();
        }, canExecuteManageServerCommands);
        
        UndoCommand = ReactiveCommand.Create(() =>
        {
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
        }, canExecuteManageServerCommands);
        
        StartServerCommand = ReactiveCommand.Create(() =>
        {
            if (CheckIfAnySettingChanged())
            {
                Console.WriteLine("Save your changes before starting the server"); // TODO: Launcher Notification
            }
            else
            {
                Server.StartCommand.Execute(null);
            }
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
        
        CheckIfAnySettingChanged();
    }
    
    private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerEntry.IsOnline))
        {
            this.RaisePropertyChanged(nameof(ServerIsOnline));
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Binding;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using NitroxServer.Serialization.World;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Nitrox.Launcher.ViewModels;

public class ManageServerViewModel : RoutableViewModelBase
{
    public static Array PlayerPerms => Enum.GetValues(typeof(PlayerPermissions));


    private ServerEntry server;
    /// <summary>
    ///     When set, navigates to the <see cref="ManageServerView" />.
    /// </summary>
    public ServerEntry Server
    {
        get => server;
        private set
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
        set => this.RaiseAndSetIfChanged( ref serverGameMode, value);
    }

    private string serverSeed;
    public string ServerSeed
    {
        get => serverSeed;
        set => this.RaiseAndSetIfChanged(ref serverSeed, value?.ToUpper());
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


    private bool HasChanges()
    {
        if (Server == null)
        {
            return false;
        }
        return ServerName != Server.Name ||
               ServerPassword != Server.Password ||
               ServerGameMode != Server.GameMode ||
               ServerSeed != Server.Seed ||
               ServerDefaultPlayerPerm != Server.DefaultPlayerPerm ||
               ServerAutoSaveInterval != Server.AutoSaveInterval ||
               ServerMaxPlayers != Server.MaxPlayers ||
               ServerPlayers != Server.Players ||
               ServerPort != Server.Port ||
               ServerAutoPortForward != Server.AutoPortForward ||
               ServerAllowLanDiscovery != Server.AllowLanDiscovery ||
               ServerAllowCommands != Server.AllowCommands;
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; init; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; init; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; init; }
    public ReactiveCommand<Unit, Unit> StartServerCommand { get; init; }

    public ReactiveCommand<Unit, Unit> AdvancedSettingsCommand { get; init; }
    public ReactiveCommand<Unit, Unit> OpenWorldFolderCommand { get; init; }
    public ReactiveCommand<Unit, Unit> RestoreBackupCommand { get; init; }
    public ReactiveCommand<Unit, Unit> DeleteServerCommand { get; init; }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        this.BindValidation();

        IObservable<bool> canExecuteSaveCommand = this.WhenAnyPropertyChanged().CombineLatest(Observable.Return(this), this.IsValid()).Select(pair => pair.Second.HasChanges() && pair.Third);
        IObservable<bool> canExecuteUndoCommand = this.WhenAnyPropertyChanged().Select(x => x.HasChanges());
        IObservable<bool> canExecuteManageServerCommands = this.WhenAnyPropertyChanged().Select(x => !x.HasChanges());

        IObservable<bool> canExecuteAdvancedSettingsButtonCommands = this.WhenAnyPropertyChanged().Select(x => !x.ServerIsOnline);

        BackCommand = ReactiveCommand.Create(() =>
        {
            Router.NavigateBack.Execute();
        }, canExecuteManageServerCommands);

        SaveCommand = ReactiveCommand.Create(() =>
        {
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
            this.RaisePropertyChanged(nameof(Server));
            
            worldFolderDirectory = Path.Combine(WorldManager.SavesFolderDir, Server.Name);
        }, canExecuteSaveCommand);

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
        }, canExecuteUndoCommand);

        StartServerCommand = ReactiveCommand.Create(() =>
        {
            Server.StartCommand.Execute(null);
        }, canExecuteManageServerCommands);


        AdvancedSettingsCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Open Advanced Settings Popup (which is automatically populated with the rest of the server.cfg settings)
        });

        OpenWorldFolderCommand = ReactiveCommand.Create(() =>
        {
            Process.Start(worldFolderDirectory)?.Dispose(); // TODO: Fix file access permission issues
        });

        RestoreBackupCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Open Restore Backup Popup
        }, canExecuteAdvancedSettingsButtonCommands);

        DeleteServerCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Delete this specific server's files
        }, canExecuteAdvancedSettingsButtonCommands);
    }
    public void LoadFrom(ServerEntry serverEntry)
    {
        Server = serverEntry;
        worldFolderDirectory = Path.Combine(WorldManager.SavesFolderDir, Server.Name);

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

    private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerEntry.IsOnline))
        {
            this.RaisePropertyChanged(nameof(ServerIsOnline));
        }
    }
}

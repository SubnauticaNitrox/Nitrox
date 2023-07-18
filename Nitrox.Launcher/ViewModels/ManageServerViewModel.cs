using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.Serialization.World;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class ManageServerViewModel : RoutableViewModelBase
{
    public static Array PlayerPerms => Enum.GetValues(typeof(Perms));

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
            SetProperty(ref server, value);
            if (server != null)
            {
                server.PropertyChanged += Server_PropertyChanged;
            }
        }
    }

    [ObservableProperty]
    private bool serverIsOnline;

    [ObservableProperty]
    private string serverName;

    [ObservableProperty]
    private string serverPassword;

    [ObservableProperty]
    private ServerGameMode serverGameMode;

    [ObservableProperty]
    private string serverSeed;

    [ObservableProperty]
    private Perms serverDefaultPlayerPerm;

    [ObservableProperty]
    private int serverAutoSaveInterval;

    [ObservableProperty]
    private int serverMaxPlayers;

    [ObservableProperty]
    private int serverPlayers;

    [ObservableProperty]
    private int serverPort;

    [ObservableProperty]
    private bool serverAutoPortForward;

    [ObservableProperty]
    private bool serverAllowLanDiscovery;

    [ObservableProperty]
    private bool serverAllowCommands;

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
               ServerDefaultPlayerPerm != Server.PlayerPermissions ||
               ServerAutoSaveInterval != Server.AutoSaveInterval ||
               ServerMaxPlayers != Server.MaxPlayers ||
               ServerPlayers != Server.Players ||
               ServerPort != Server.Port ||
               ServerAutoPortForward != Server.AutoPortForward ||
               ServerAllowLanDiscovery != Server.AllowLanDiscovery ||
               ServerAllowCommands != Server.AllowCommands;
    }

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // TODO: Use CanExecute to enable/disable buttons instead of these observables.
        // IObservable<bool> canExecuteSaveCommand = this.WhenAnyPropertyChanged().CombineLatest(Observable.Return(this), this.IsValid()).Select(pair => pair.Second.HasChanges() && pair.Third);
        // IObservable<bool> canExecuteUndoCommand = this.WhenAnyPropertyChanged().Select(x => x.HasChanges());
        // IObservable<bool> canExecuteManageServerCommands = this.WhenAnyPropertyChanged().Select(x => !x.HasChanges());
        // IObservable<bool> canExecuteAdvancedSettingsButtonCommands = this.WhenAnyPropertyChanged().Select(x => !x.ServerIsOnline);
    }

    [RelayCommand]
    private void Back()
    {
        Router.NavigateBack.Execute();
    }

    [RelayCommand]
    private void Save()
    {
        Server.Name = ServerName;
        Server.Password = ServerPassword;
        Server.GameMode = ServerGameMode;
        Server.Seed = ServerSeed;
        Server.PlayerPermissions = ServerDefaultPlayerPerm;
        Server.AutoSaveInterval = ServerAutoSaveInterval;
        Server.MaxPlayers = ServerMaxPlayers;
        Server.Players = ServerPlayers;
        Server.Port = ServerPort;
        Server.AutoPortForward = ServerAutoPortForward;
        Server.AllowLanDiscovery = ServerAllowLanDiscovery;
        Server.AllowCommands = ServerAllowCommands;
        this.RaisePropertyChanged(nameof(Server));

        worldFolderDirectory = Path.Combine(WorldManager.SavesFolderDir, Server.Name);
    }

    [RelayCommand]
    private void Undo()
    {
        ServerName = Server.Name;
        ServerPassword = Server.Password;
        ServerGameMode = Server.GameMode;
        ServerSeed = Server.Seed;
        ServerDefaultPlayerPerm = Server.PlayerPermissions;
        ServerAutoSaveInterval = Server.AutoSaveInterval;
        ServerMaxPlayers = Server.MaxPlayers;
        ServerPlayers = Server.Players;
        ServerPort = Server.Port;
        ServerAutoPortForward = Server.AutoPortForward;
        ServerAllowLanDiscovery = Server.AllowLanDiscovery;
        ServerAllowCommands = Server.AllowCommands;
    }

    [RelayCommand]
    private void StartServer()
    {
        Server.Start();
    }

    [RelayCommand]
    private void OpenAdvancedSettings()
    {
        // TODO: Open Advanced Settings Popup (which is automatically populated with the rest of the server.cfg settings)
    }

    [RelayCommand]
    private void OpenWorldFolder()
    {
        Process.Start(worldFolderDirectory)?.Dispose(); // TODO: Fix file access permission issues
    }

    [RelayCommand]
    private void RestoreBackup()
    {
        // TODO: Open Restore Backup Popup
    }

    [RelayCommand]
    private void DeleteServer()
    {
        // TODO: Delete this specific server's files after showing a confirmation popup
        WorldManager.DeleteSave(worldFolderDirectory);
        Router.NavigateBack.Execute();
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
        ServerDefaultPlayerPerm = Server.PlayerPermissions;
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

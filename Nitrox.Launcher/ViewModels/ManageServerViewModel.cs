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
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private string serverName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private string serverPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private ServerGameMode serverGameMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private string serverSeed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private Perms serverDefaultPlayerPerm;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private int serverAutoSaveInterval;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private int serverMaxPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private int serverPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private int serverPort;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private bool serverAutoPortForward;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private bool serverAllowLanDiscovery;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand))]
    private bool serverAllowCommands;

    private bool ServerIsOnline => Server.IsOnline;

    private string worldFolderDirectory;

    private bool HasChanges()
    {
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
    }

    [RelayCommand]
    private void Back()
    {
        Router.NavigateBack.Execute();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
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

        worldFolderDirectory = Path.Combine(WorldManager.SavesFolderDir, Server.Name);

        UndoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave()
    {
        // TODO: Add IsValid check from MVVM toolkit's validation API
        return !ServerIsOnline && HasChanges();
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
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

    private bool CanUndo()
    {
        return !ServerIsOnline && HasChanges();
    }

    [RelayCommand]
    public void StartServer()
    {
        Server.Start();

        UndoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public void StopServer()
    {
        Server.Stop();

        UndoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
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
            OnPropertyChanged(nameof(ServerIsOnline));
        }
    }
}

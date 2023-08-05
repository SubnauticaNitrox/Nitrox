using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Validators;
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
    public string OriginalServerName => Server?.Name;

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
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NitroxUniqueSaveName(nameof(OriginalServerName))]
    private string serverName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private string serverPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private ServerGameMode serverGameMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [NitroxWorldSeed]
    private string serverSeed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private Perms serverDefaultPlayerPerm;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(10, 86400, ErrorMessage = "Value must be between 10s and 24 hours (86400s).")]
    private int serverAutoSaveInterval;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [Range(1, 1000)]
    [NotifyDataErrorInfo]
    private int serverMaxPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private int serverPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(ushort.MinValue, ushort.MaxValue)]
    private int serverPort;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAutoPortForward;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAllowLanDiscovery;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAllowCommands;

    private bool ServerIsOnline => Server.IsOnline;

    private string worldFolderDirectory;

    private bool HasChanges() => ServerName != Server.Name ||
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

    public ManageServerViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    private void Back()
    {
        Router.NavigateBack.Execute();
    }

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    public void StartServer()
    {
        Server.Start();

        RestoreBackupCommand.NotifyCanExecuteChanged();
        DeleteServerCommand.NotifyCanExecuteChanged();
    }

    private bool CanGoBackAndStartServer() =>!HasChanges();

    [RelayCommand]
    public void StopServer()
    {
        Server.Stop();

        RestoreBackupCommand.NotifyCanExecuteChanged();
        DeleteServerCommand.NotifyCanExecuteChanged();
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

        Server.SaveSettings(OriginalServerName);
        //OriginalServerName = Server.Name;

        worldFolderDirectory = Path.Combine(WorldManager.SavesFolderDir, Server.Name);

        BackCommand.NotifyCanExecuteChanged();
        StartServerCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() => !HasErrors && !ServerIsOnline && HasChanges();

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

    private bool CanUndo() => !ServerIsOnline && HasChanges();

    [RelayCommand]
    private void OpenAdvancedSettings()
    {
        // TODO: Open Advanced Settings Popup (which is automatically populated with the rest of the server.cfg settings)
    }

    [RelayCommand]
    private void OpenWorldFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = worldFolderDirectory,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackupAndDeleteServer))]
    private void RestoreBackup()
    {
        // TODO: Open Restore Backup Popup
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackupAndDeleteServer))]
    private void DeleteServer()
    {
        // TODO: Delete this specific server's files after showing a confirmation popup
        WorldManager.DeleteSave(worldFolderDirectory);
        Router.NavigateBack.Execute();
    }

    private bool CanRestoreBackupAndDeleteServer() => !ServerIsOnline;

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

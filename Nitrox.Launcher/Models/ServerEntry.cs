using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Messages;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Upgrade;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
    private static readonly ServerConfig serverDefaults = new();

    [ObservableProperty]
    private bool isOnline;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string password;
    [ObservableProperty]
    private string seed;
    [ObservableProperty]
    private ServerGameMode gameMode = serverDefaults.GameMode;
    [ObservableProperty]
    private Perms playerPermissions = serverDefaults.DefaultPlayerPerm;
    [ObservableProperty]
    private int autoSaveInterval = serverDefaults.SaveInterval/1000;
    [ObservableProperty]
    private int players;
    [ObservableProperty]
    private int maxPlayers = serverDefaults.MaxConnections;
    [ObservableProperty]
    private int port = serverDefaults.ServerPort;
    [ObservableProperty]
    private bool autoPortForward = serverDefaults.AutoPortForward;
    [ObservableProperty]
    private bool allowLanDiscovery = serverDefaults.LANDiscoveryEnabled;
    [ObservableProperty]
    private bool allowCommands = !serverDefaults.DisableConsole;
    /// <summary>
    /// TODO: This should be inferred from the server having a save file or not.
    /// </summary>
    [ObservableProperty]
    private bool isNewServer = true;
    [ObservableProperty]
    private Version version = NitroxEnvironment.Version;

    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    public void Start()
    {
        IsNewServer = false;
        IsOnline = true;
    }

    private bool CanStart() => Version >= SaveDataUpgrade.MinimumSaveVersion && Version <= NitroxEnvironment.Version;

    [RelayCommand]
    public void Stop()
    {
        IsOnline = false;
    }
}

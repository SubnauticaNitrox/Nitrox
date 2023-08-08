using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Messages;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
    private static readonly SubnauticaServerConfig serverDefaults = new();

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
    [ObservableProperty]
    private bool isNewServer = true;
    [ObservableProperty]
    private DateTime lastAccessedTime = DateTime.Now;

    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));
    }

    [RelayCommand]
    public void Start()
    {
        IsNewServer = false;
        IsOnline = true;
    }

    [RelayCommand]
    public void Stop()
    {
        IsOnline = false;
    }
}

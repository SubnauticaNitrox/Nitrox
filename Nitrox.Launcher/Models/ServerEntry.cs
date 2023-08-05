using System;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Messages;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.Serialization.Upgrade;
using NitroxServer.Serialization.World;

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
    private Version version = NitroxEnvironment.Version;
    
    [ObservableProperty]
    private bool isNewServer = true;
    [ObservableProperty]
    private string saveFileDirectory;
    
    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;

        SaveFileDirectory = Path.Combine(WorldManager.SavesFolderDir, Name ?? "");
        //if (File.Exists(Path.Combine(SaveFileDirectory, "WorldData.json")))
        //{
        //    IsNewServer = false;
        //}
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    public void Start()
    {
        //
        IsNewServer = false;
        IsOnline = true;
    }

    private bool CanStart() => Version >= SaveDataUpgrade.MinimumSaveVersion && Version <= NitroxEnvironment.Version;

    [RelayCommand]
    public void Stop()
    {
        IsOnline = false;
    }

    public void SaveSettings(string originalSaveName)
    {
        // If world name was changed, rename save folder to match it (DOESN'T WORK)
        
        //SaveFileDirectory = WorldManager.ChangeSaveName(originalSaveName, Name);  // Try 1
        
        //WorldManager.ChangeSaveName(originalSaveName, Name);  // Try 2
        //SaveFileDirectory = Path.Combine(WorldManager.SavesFolderDir, Name);
        
        string oldDir = Path.Combine(WorldManager.SavesFolderDir, originalSaveName);  // Try 3
        SaveFileDirectory = Path.Combine(WorldManager.SavesFolderDir, Name);
        if (oldDir != SaveFileDirectory)
        {
            Directory.Move(oldDir, SaveFileDirectory); // These two lines are needed to handle names that change in capitalization,
            //Directory.Move($"{SaveFileDirectory} temp", SaveFileDirectory);   // since Windows still thinks of the two names as the same.
        }

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(SaveFileDirectory);
        using (config.Update(SaveFileDirectory))
        {
            config.SaveName = Name;
            config.ServerPassword = Password;
            if (IsNewServer) { config.Seed = Seed; }
            config.GameMode = GameMode;
            config.DefaultPlayerPerm = PlayerPermissions;
            config.SaveInterval = AutoSaveInterval*1000;  // Convert seconds to milliseconds
            config.MaxConnections = MaxPlayers;
            config.ServerPort = Port;
            config.AutoPortForward = AutoPortForward;
            config.LANDiscoveryEnabled = AllowLanDiscovery;
            config.DisableConsole = !AllowCommands;
        }
    }
}

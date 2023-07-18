using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Nitrox.Launcher.ViewModels;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Upgrade;
using ReactiveUI;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public class ServerEntry : ReactiveObject
{
    private static readonly ServerConfig serverDefaults = new();
    
    private bool isOnline;
    private string name;
    private string password;
    private string seed;
    private ServerGameMode gamemode = serverDefaults.GameMode;
    private Perms playerPermissions = serverDefaults.DefaultPlayerPerm;
    private int autoSaveInterval = serverDefaults.SaveInterval/1000;
    private int players;
    private int maxPlayers = serverDefaults.MaxConnections;
    private int port = serverDefaults.ServerPort;
    private bool autoPortForward = serverDefaults.AutoPortForward;
    private bool allowLanDiscovery = serverDefaults.LANDiscoveryEnabled;
    private bool allowCommands = !serverDefaults.DisableConsole;
    private bool isNewServer = true;
    private Version version = NitroxEnvironment.Version;

    public bool IsOnline
    {
        get => isOnline;
        set => this.RaiseAndSetIfChanged(ref isOnline, value);
    }
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }
    public string Password
    {
        get => password;
        set => this.RaiseAndSetIfChanged(ref password, value);
    }
    public string Seed
    {
        get => seed;
        set => this.RaiseAndSetIfChanged(ref seed, value);
    }
    public ServerGameMode GameMode
    {
        get => gamemode;
        set => this.RaiseAndSetIfChanged(ref gamemode, value);
    }
    public Perms DefaultPlayerPerm
    {
        get => playerPermissions;
        set => this.RaiseAndSetIfChanged(ref playerPermissions, value);
    }
    public int AutoSaveInterval
    {
        get => autoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref autoSaveInterval, value);
    }
    public int Players
    {
        get => players;
        set => this.RaiseAndSetIfChanged(ref players, value);
    }
    public int MaxPlayers
    {
        get => maxPlayers;
        set => this.RaiseAndSetIfChanged(ref maxPlayers, value);
    }
    public int Port
    {
        get => port;
        set => this.RaiseAndSetIfChanged(ref port, value);
    }
    public bool AutoPortForward
    {
        get => autoPortForward;
        set => this.RaiseAndSetIfChanged(ref autoPortForward, value);
    }
    public bool AllowLanDiscovery
    {
        get => allowLanDiscovery;
        set => this.RaiseAndSetIfChanged(ref allowLanDiscovery, value);
    }
    public bool AllowCommands
    {
        get => allowCommands;
        set => this.RaiseAndSetIfChanged(ref allowCommands, value);
    }

    /// <summary>
    /// TODO: This should be inferred from the server having a save file or not.
    /// </summary>
    public bool IsNewServer
    {
        get => isNewServer;
        set => this.RaiseAndSetIfChanged(ref isNewServer, value);
    }

    public Version Version
    {
        get => version;
        init => this.RaiseAndSetIfChanged(ref version, value);
    }

    public ICommand StartCommand { get; init; }
    public ICommand StopCommand { get; init; }

    public ServerEntry()
    {
        IObservable<bool> canExecuteStartServerCommand = this.WhenAnyValue(x => x.Version, (serverVersion) => serverVersion >= SaveDataUpgrade.MinimumSaveVersion && serverVersion <= NitroxEnvironment.Version );
        
        StartCommand = ReactiveCommand.CreateFromTask(StartServer, canExecuteStartServerCommand);
        StopCommand = ReactiveCommand.CreateFromTask(StopServer);
    }

    private Task StartServer()
    {
        IsOnline = true;
        if (IsNewServer)
            IsNewServer = false;
        return Task.CompletedTask;
    }

    private Task StopServer()
    {
        IsOnline = false;
        return Task.CompletedTask;
    }
}

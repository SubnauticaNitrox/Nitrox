using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public class ServerEntry : ReactiveObject
{
    // Default values
    private bool isOnline;
    private string name;
    private string password;
    private string seed;
    private GameMode gamemode = GameMode.SURVIVAL;
    private PlayerPermissions playerPermissions = PlayerPermissions.PLAYER;
    private int autoSaveInterval = 120;
    private int players;
    private int maxPlayers = 100;
    private int port = 11000;
    private bool autoPortForward = true;
    private bool allowLanDiscovery = true;
    private bool allowCommands = true;
    
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
    public GameMode GameMode
    {
        get => gamemode;
        set => this.RaiseAndSetIfChanged(ref gamemode, value);
    }
    public PlayerPermissions DefaultPlayerPerm
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
    public ICommand StartCommand { get; init; }
    public ICommand StopCommand { get; init; }

    public ServerEntry()
    {
        StartCommand = ReactiveCommand.CreateFromTask(StartServer);
        StopCommand = ReactiveCommand.CreateFromTask(StopServer);
    }

    private Task StartServer()
    {
        IsOnline = true;
        return Task.CompletedTask;
    }

    private Task StopServer()
    {
        IsOnline = false;
        return Task.CompletedTask;
    }
}

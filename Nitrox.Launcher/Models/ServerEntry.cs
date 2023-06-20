using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public class ServerEntry : ReactiveObject
{
    private bool isOnline;
    private string name;
    private string password;
    private string seed;
    private PlayerPermissions playerPermissions;
    private int autoSaveInterval;
    private int port;
    private bool autoPortForward;
    private bool allowLanDiscovery;
    private bool allowCommands;
    
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
    public GameMode GameMode { get; init; }
    public PlayerPermissions DefaultPlayerPerm
    {
        get => playerPermissions = PlayerPermissions.PLAYER;
        set => this.RaiseAndSetIfChanged(ref playerPermissions, value);
    }
    public int AutoSaveInterval
    {
        get => autoSaveInterval = 120;
        set => this.RaiseAndSetIfChanged(ref autoSaveInterval, value);
    }
    public int Players { get; init; }
    public int MaxPlayers { get; init; } = 100;
    public int Port
    {
        get => port = 11000;
        set => this.RaiseAndSetIfChanged(ref port, value);
    }
    public bool AutoPortForward
    {
        get => autoPortForward = true;
        set => this.RaiseAndSetIfChanged(ref autoPortForward, value);
    }
    public bool AllowLanDiscovery
    {
        get => allowLanDiscovery = true;
        set => this.RaiseAndSetIfChanged(ref allowLanDiscovery, value);
    }
    public bool AllowCommands
    {
        get => allowCommands = true;
        set => this.RaiseAndSetIfChanged(ref allowCommands, value);
    }
    public string SavePath { get; init; }
    public ICommand StartCommand { get; init; }
    public ICommand StopCommand { get; init; }
    public ICommand ManageCommand { get; init; }

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

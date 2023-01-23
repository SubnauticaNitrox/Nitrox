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

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public bool IsOnline
    {
        get => isOnline;
        set => this.RaiseAndSetIfChanged(ref isOnline, value);
    }

    public GameMode GameMode { get; init; }
    public int Players { get; init; }
    public int MaxPlayers { get; init; } = 100;
    public ICommand StopCommand { get; init; }
    public ICommand ManageCommand { get; init; }

    public ServerEntry()
    {
        StopCommand = ReactiveCommand.Create(() => IsOnline = false);
    }
}

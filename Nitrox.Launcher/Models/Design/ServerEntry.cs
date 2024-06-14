using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Exceptions;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
    // ServerEntry Variables
    private static readonly SubnauticaServerConfig serverDefaults = new();

    [ObservableProperty]
    private bool allowCommands = !serverDefaults.DisableConsole;

    [ObservableProperty]
    private bool allowLanDiscovery = serverDefaults.LANDiscoveryEnabled;

    [ObservableProperty]
    private bool autoPortForward = serverDefaults.AutoPortForward;

    [ObservableProperty]
    private int autoSaveInterval = serverDefaults.SaveInterval / 1000;

    [ObservableProperty]
    private NitroxGameMode gameMode = serverDefaults.GameMode;

    [ObservableProperty]
    private bool isNewServer = true;

    [ObservableProperty]
    private bool isOnline;

    [ObservableProperty]
    private DateTime lastAccessedTime = DateTime.Now;

    [ObservableProperty]
    private int maxPlayers = serverDefaults.MaxConnections;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private Perms playerPermissions = serverDefaults.DefaultPlayerPerm;

    [ObservableProperty]
    private int players;

    [ObservableProperty]
    private int port = serverDefaults.ServerPort;

    [ObservableProperty]
    private string seed;

    [ObservableProperty]
    private Bitmap serverIcon;
    
    [ObservableProperty]
    private Version version;

    private ServerProcess serverProcess;

    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    [RelayCommand]
    public void Start()
    {
        try
        {
            if (serverProcess?.IsRunning ?? false)
            {
                throw new DuplicateSingularApplicationException("Nitrox Server");
            }
            // Start server and add notify when server closed.
            serverProcess = ServerProcess.Start(Name, () => Dispatcher.UIThread.InvokeAsync(StopAsync));
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error(ex.Message);
            Log.Error(ex);
            return;
        }

        IsNewServer = false;
        IsOnline = true;
    }

    [RelayCommand]
    public async Task<bool> StopAsync()
    {
        if (serverProcess == null || await serverProcess.CloseAsync())
        {
            IsOnline = false;
            return true;
        }

        return false;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));
    }

    private class ServerProcess : IDisposable
    {
        private Process serverProcess;
        private NamedPipeClientStream commandStream;
        public string SaveName { get; }
        public bool IsRunning => !serverProcess?.HasExited ?? false;
        public string SaveDir => Path.Combine(ServersViewModel.SavesFolderDir, SaveName);

        private ServerProcess(string saveName, Action onExited)
        {
            SaveName = saveName;

            string serverPath = Path.Combine(NitroxUser.CurrentExecutablePath, "Server", "NitroxServer-Subnautica.exe");
            ProcessStartInfo startInfo = new(serverPath)
            {
                WorkingDirectory = NitroxUser.CurrentExecutablePath,
                Verb = "open",
                Arguments = $@"""{SaveDir}"""
            };

            serverProcess = Process.Start(startInfo);
            if (serverProcess != null)
            {
                serverProcess.EnableRaisingEvents = true; // Required for 'Exited' event from process.
                serverProcess.Exited += (_, _) =>
                {
                    onExited?.Invoke();
                };
            }

            if (File.Exists(Path.Combine(SaveDir, "WorldData.json")))
            {
                File.SetLastWriteTime(Path.Combine(SaveDir, "WorldData.json"), DateTime.Now);
            }
        }

        public static ServerProcess Start(string saveName, Action onExited)
        {
            return new(saveName, onExited);
        }

        public async Task<bool> CloseAsync()
        {
            try
            {
                // TODO: Fix the server to always handle stop command, even when it's still starting up.
                await SendCommandAsync("stop");
            }
            catch (TimeoutException)
            {
                // server could be dead, ignore
            }

            Dispose();
            return true;
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsRunning)
            {
                return;
            }

            commandStream ??= new NamedPipeClientStream(".", $"Nitrox Server {serverProcess.Id}", PipeDirection.Out, PipeOptions.Asynchronous);
            if (!commandStream.IsConnected)
            {
                await commandStream.ConnectAsync(5000);
            }
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            await commandStream.WriteAsync(BitConverter.GetBytes((uint)commandBytes.Length));
            await commandStream.WriteAsync(commandBytes);
        }

        public void Dispose()
        {
            serverProcess?.Dispose();
            serverProcess = null;
        }
    }
}

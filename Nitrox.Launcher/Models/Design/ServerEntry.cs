using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Exceptions;
using Nitrox.Launcher.Models.Utils;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
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

    private ServerProcess serverProcess;

    [ObservableProperty]
    private Version version;

    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    public static ServerEntry FromDirectory(string saveDir)
    {
        ServerEntry result = new();
        result.RefreshFromDirectory(saveDir);
        return result;
    }

    public void RefreshFromDirectory(string saveDir)
    {
        if (!File.Exists(Path.Combine(saveDir, "server.cfg")) || !File.Exists(Path.Combine(saveDir, "Version.json")))
        {
            return;
        }

        Bitmap serverIcon = null;
        string serverIconPath = Path.Combine(saveDir, "servericon.png");
        if (File.Exists(serverIconPath))
        {
            serverIcon = new Bitmap(Path.Combine(saveDir, "servericon.png"));
        }

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = config.SerializerMode switch
        {
            ServerSerializerMode.JSON => "json",
            ServerSerializerMode.PROTOBUF => "nitrox",
            _ => throw new NotImplementedException()
        };

        Version version;
        using (FileStream stream = new(Path.Combine(saveDir, $"Version.{fileEnding}"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            version = new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version;
        }

        Name = Path.GetFileName(saveDir);
        ServerIcon = serverIcon;
        Password = config.ServerPassword;
        Seed = config.Seed;
        GameMode = config.GameMode;
        PlayerPermissions = config.DefaultPlayerPerm;
        AutoSaveInterval = config.SaveInterval / 1000;
        MaxPlayers = config.MaxConnections;
        Port = config.ServerPort;
        AutoPortForward = config.AutoPortForward;
        AllowLanDiscovery = config.LANDiscoveryEnabled;
        AllowCommands = !config.DisableConsole;
        IsNewServer = !File.Exists(Path.Combine(saveDir, "WorldData.json"));
        Version = version;
        LastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(saveDir, $"WorldData.{fileEnding}"))
                                                     ?
                                                     // This file is affected by server saving
                                                     Path.Combine(saveDir, $"WorldData.{fileEnding}")
                                                     :
                                                     // If the above file doesn't exist (server was never ran), use the Version file instead
                                                     Path.Combine(saveDir, $"Version.{fileEnding}"));

        // Handle and correct cases where config save name does not match folder name.
        if (Name != config.SaveName)
        {
            using (config.Update(saveDir))
            {
                config.SaveName = Name;
            }
        }
    }

    public void Start(string savesDir)
    {
        if (!Directory.Exists(savesDir))
        {
            throw new DirectoryNotFoundException($"Directory '{savesDir}' not found");
        }

        try
        {
            if (serverProcess?.IsRunning ?? false)
            {
                throw new DuplicateSingularApplicationException("Nitrox Server");
            }
            // Start server and add notify when server closed.
            serverProcess = ServerProcess.Start(Path.Combine(savesDir, Name), () => Dispatcher.UIThread.InvokeAsync(StopAsync));
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

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));

    private class ServerProcess : IDisposable
    {
        private NamedPipeClientStream commandStream;
        private Process serverProcess;
        public bool IsRunning => !serverProcess?.HasExited ?? false;

        private ServerProcess(string saveDir, Action onExited)
        {
            string serverExeName = "NitroxServer-Subnautica.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                serverExeName = "NitroxServer-Subnautica";
            }
            string serverPath = Path.Combine(NitroxUser.CurrentExecutablePath, "server", serverExeName);
            ProcessStartInfo startInfo = new(serverPath)
            {
                WorkingDirectory = NitroxUser.CurrentExecutablePath,
                Verb = "open",
                Arguments = $@"""{saveDir}"""
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

            if (File.Exists(Path.Combine(saveDir, "WorldData.json")))
            {
                File.SetLastWriteTime(Path.Combine(saveDir, "WorldData.json"), DateTime.Now);
            }
        }

        public static ServerProcess Start(string saveDir, Action onExited) => new(saveDir, onExited);

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

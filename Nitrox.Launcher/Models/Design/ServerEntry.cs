using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Exceptions;
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

    [ObservableProperty]
    private bool isEmbedded;

    internal ServerProcess Process { get; private set; }

    [ObservableProperty]
    private Version version = NitroxEnvironment.Version;

    public static ServerEntry FromDirectory(string saveDir)
    {
        ServerEntry result = new();
        return result.RefreshFromDirectory(saveDir) ? result : null;
    }

    public static ServerEntry CreateNew(string saveDir, NitroxGameMode saveGameMode)
    {
        Directory.CreateDirectory(saveDir);
        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = "json";
        if (config.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = "nitrox";
        }

        File.WriteAllText(Path.Combine(saveDir, $"Version.{fileEnding}"), null);
        using (config.Update(saveDir))
        {
            config.GameMode = saveGameMode;
        }
        return FromDirectory(saveDir);
    }

    public bool RefreshFromDirectory(string saveDir)
    {
        if (!File.Exists(Path.Combine(saveDir, "server.cfg")) || !File.Exists(Path.Combine(saveDir, "Version.json")))
        {
            Log.Warn($"Tried loading invalid save directory at '{saveDir}'");
            return false;
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
        IsEmbedded = config.IsEmbedded || RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Force embedded on MacOS
        LastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(saveDir, $"WorldData.{fileEnding}"))
                                                     ?
                                                     // This file is affected by server saving
                                                     Path.Combine(saveDir, $"WorldData.{fileEnding}")
                                                     :
                                                     // If the above file doesn't exist (server was never ran), use the Version file instead
                                                     Path.Combine(saveDir, $"Version.{fileEnding}"));
        return true;
    }

    public void Start(string savesDir)
    {
        if (!Directory.Exists(savesDir))
        {
            throw new DirectoryNotFoundException($"Directory '{savesDir}' not found");
        }
        if (Process?.IsRunning ?? false)
        {
            throw new DuplicateSingularApplicationException("Nitrox Server");
        }
        // Start server and add notify when server closed.
        Process = ServerProcess.Start(Path.Combine(savesDir, Name), () => Dispatcher.UIThread.InvokeAsync(StopAsync), IsEmbedded);

        IsNewServer = false;
        IsOnline = true;
    }

    [RelayCommand]
    public async Task<bool> StopAsync()
    {
        if (Process is not { IsRunning: true })
        {
            IsOnline = false;
            return true;
        }
        if (await Process.CloseAsync())
        {
            IsOnline = false;
            return true;
        }

        return false;
    }

    [RelayCommand]
    public void OpenSaveFolder()
    {
        System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name),
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }

    internal class ServerProcess : IDisposable
    {
        private NamedPipeClientStream commandStream;
        private Process serverProcess;
        public bool IsRunning => !serverProcess?.HasExited ?? false;
        public AvaloniaList<string> Output { get; } = new();

        private ServerProcess(string saveDir, Action onExited, bool captureOutput = false)
        {
            string serverExeName = "NitroxServer-Subnautica.exe";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serverExeName = "NitroxServer-Subnautica";
            }
            string serverFile = Path.Combine(NitroxUser.ExecutableRootPath, serverExeName);
            ProcessStartInfo startInfo = new(serverFile)
            {
                WorkingDirectory = NitroxUser.ExecutableRootPath,
                ArgumentList = { "--save", Path.GetFileName(saveDir) },
                RedirectStandardOutput = captureOutput,
                RedirectStandardError = captureOutput,
                RedirectStandardInput = captureOutput,
                WindowStyle = captureOutput ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                CreateNoWindow = captureOutput
            };
            Log.Info($"Starting server:{Environment.NewLine}File: {startInfo.FileName}{Environment.NewLine}Working directory: {startInfo.WorkingDirectory}{Environment.NewLine}Arguments: {string.Join(", ", startInfo.ArgumentList)}");

            serverProcess = System.Diagnostics.Process.Start(startInfo);
            if (serverProcess != null)
            {
                serverProcess.EnableRaisingEvents = true; // Required for 'Exited' event from process.
                if (captureOutput)
                {
                    serverProcess.OutputDataReceived += (sender, args) => Output.Add(args.Data ?? "");
                    serverProcess.BeginOutputReadLine();
                }
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

        public static ServerProcess Start(string saveDir, Action onExited, bool isEmbedded) => new(saveDir, onExited, isEmbedded);

        /// <summary>
        ///     Tries to close the server gracefully with a timeout of 30 seconds. If it fails, returns false.
        /// </summary>
        public async Task<bool> CloseAsync()
        {
            using CancellationTokenSource ctsCloseTimeout = new(TimeSpan.FromSeconds(30));
            try
            {
                do
                {
                    if (!await SendCommandAsync("stop"))
                    {
                        await Task.Delay(100, ctsCloseTimeout.Token);
                    }
                } while (IsRunning && !ctsCloseTimeout.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }

            if (IsRunning)
            {
                return false;
            }
            Dispose();
            return true;
        }

        public async Task<bool> SendCommandAsync(string command)
        {
            if (!IsRunning || string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            try
            {
                commandStream ??= new NamedPipeClientStream(".", $"Nitrox Server {serverProcess.Id}", PipeDirection.Out, PipeOptions.Asynchronous);
                if (!commandStream.IsConnected)
                {
                    await commandStream.ConnectAsync(5000);
                }
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                await commandStream.WriteAsync(BitConverter.GetBytes((uint)commandBytes.Length));
                await commandStream.WriteAsync(commandBytes);
                return true;
            }
            catch (TimeoutException)
            {
                // ignored
            }
            catch (IOException)
            {
                // ignored - "broken pipe" or "socket shutdown"
            }
            return false;
        }

        public void Dispose()
        {
            try
            {
                commandStream?.Dispose();
            }
            catch
            {
                // ignored
            }
            serverProcess?.Dispose();
            serverProcess = null;
        }
    }
}

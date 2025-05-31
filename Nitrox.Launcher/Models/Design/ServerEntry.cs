using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    public const string DEFAULT_SERVER_ICON_NAME = "servericon.png";
    public const string DEFAULT_SERVER_CONFIG_NAME = "server.cfg";

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
    private bool isEmbedded;

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
    private Version version = NitroxEnvironment.Version;

    [ObservableProperty]
    private bool isServerClosing = false;

    internal ServerProcess Process { get; private set; }

    public static ServerEntry FromDirectory(string saveDir)
    {
        ServerEntry result = new();
        return result.RefreshFromDirectory(saveDir) ? result : null;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsOnline):
                WeakReferenceMessenger.Default.Send(new ServerStatusMessage(this, IsOnline));
                break;
        }
        base.OnPropertyChanged(e);
    }

    public static ServerEntry CreateNew(string saveDir, NitroxGameMode saveGameMode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveDir, nameof(saveDir));

        Directory.CreateDirectory(saveDir);

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = config.SerializerMode switch
        {
            ServerSerializerMode.JSON => ServerJsonSerializer.FILE_ENDING,
            ServerSerializerMode.PROTOBUF => ServerProtoBufSerializer.FILE_ENDING,
            _ => throw new NotImplementedException()
        };

        File.WriteAllText(Path.Combine(saveDir, $"Version{fileEnding}"), null);
        using (config.Update(saveDir))
        {
            config.GameMode = saveGameMode;
        }

        return FromDirectory(saveDir);
    }

    public bool RefreshFromDirectory(string saveDir)
    {
        if (!File.Exists(Path.Combine(saveDir, DEFAULT_SERVER_CONFIG_NAME)))
        {
            Log.Warn($"Tried loading invalid save directory at '{saveDir}'");
            return false;
        }

        Bitmap serverIcon = null;
        string serverIconPath = Path.Combine(saveDir, DEFAULT_SERVER_ICON_NAME);
        if (File.Exists(serverIconPath))
        {
            serverIcon = new Bitmap(Path.Combine(saveDir, DEFAULT_SERVER_ICON_NAME));
        }

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = config.SerializerMode switch
        {
            ServerSerializerMode.JSON => ServerJsonSerializer.FILE_ENDING,
            ServerSerializerMode.PROTOBUF => ServerProtoBufSerializer.FILE_ENDING,
            _ => throw new NotImplementedException()
        };

        string saveFileVersion = Path.Combine(saveDir, $"Version{fileEnding}");
        if (!File.Exists(saveFileVersion))
        {
            Log.Warn($"Tried loading invalid save directory at '{saveDir}', Version file is missing");
            return false;
        }

        Version version;
        using (FileStream stream = new(saveFileVersion, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            version = config.SerializerMode switch
            {
                ServerSerializerMode.JSON => new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version,
                ServerSerializerMode.PROTOBUF => new ServerProtoBufSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version,
                _ => throw new NotImplementedException()
            };
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
        IsNewServer = !File.Exists(Path.Combine(saveDir, $"PlayerData{fileEnding}"));
        Version = version;
        IsEmbedded = config.IsEmbedded || RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Force embedded on MacOS
        LastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(saveDir, $"PlayerData{fileEnding}"))
                                                     ?
                                                     // This file is affected by server saving
                                                     Path.Combine(saveDir, $"PlayerData{fileEnding}")
                                                     :
                                                     // If the above file doesn't exist (server was never ran), use the Version file instead
                                                     Path.Combine(saveDir, $"Version{fileEnding}"));
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
    public async Task StopAsync()
    {
        IsServerClosing = true;
        if (Process is { IsRunning: true } && !await Process.CloseAsync())
        {
            // Force shutdown if server doesn't respond to close command.
            Log.Warn($"Server '{Name}' didn't respond to close command. Forcing shutdown.");
            Process.Dispose();
        }
        
        IsOnline = false;
        IsServerClosing = false;
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

    internal partial class ServerProcess : IDisposable
    {
        private OutputLineType lastOutputType;
        private Process serverProcess;
        private Ipc.ClientIpc ipc;

        [GeneratedRegex(@"^\[(?<timestamp>\d{2}:\d{2}:\d{2}\.\d{3})\]\s\[(?<level>\w+)\](?<logText>.*)?$")]
        private static partial Regex OutputLineRegex { get; }

        public bool IsRunning => !serverProcess?.HasExited ?? false; // Will need to be changed to use IPC
        public AvaloniaList<OutputLine> Output { get; } = [];

        private ServerProcess(string saveDir, Action onExited, bool isEmbeddedMode = false)
        {
            string saveName = Path.GetFileName(saveDir);
            
            string serverExeName = "NitroxServer-Subnautica.exe";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serverExeName = "NitroxServer-Subnautica";
            }
            string serverFile = Path.Combine(NitroxUser.ExecutableRootPath, serverExeName);
            ProcessStartInfo startInfo = new(serverFile)
            {
                WorkingDirectory = NitroxUser.ExecutableRootPath,
                ArgumentList =
                {
                    "--save",
                    saveName
                },
                RedirectStandardOutput = isEmbeddedMode,
                RedirectStandardError = isEmbeddedMode,
                RedirectStandardInput = isEmbeddedMode,
                WindowStyle = isEmbeddedMode ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                CreateNoWindow = isEmbeddedMode
            };
            if (isEmbeddedMode)
            {
                startInfo.ArgumentList.Add("--embedded");
            }
            Log.Info($"Starting server:{Environment.NewLine}File: {startInfo.FileName}{Environment.NewLine}Working directory: {startInfo.WorkingDirectory}{Environment.NewLine}Arguments: {string.Join(", ", startInfo.ArgumentList)}");

            serverProcess = System.Diagnostics.Process.Start(startInfo); // Might need to be changed for when the launcher reconnects to a running server
            if (serverProcess != null)
            {
                ipc = new Ipc.ClientIpc(serverProcess.Id, new CancellationTokenSource());
                serverProcess.EnableRaisingEvents = true; // Required for 'Exited' event from process.
                if (isEmbeddedMode)
                {
                    serverProcess.OutputDataReceived += (_, args) => // Replace this with IPC (so that it works when the launcher reconnects to a running server)
                    {
                        if (args.Data == null)
                        {
                            return;
                        }

                        Match match = OutputLineRegex.Match(args.Data);
                        if (match.Success)
                        {
                            OutputLine outputLine = new()
                            {
                                Timestamp = $"[{match.Groups["timestamp"].ValueSpan}]",
                                LogText = match.Groups["logText"].ValueSpan.Trim().ToString(),
                                Type = match.Groups["level"].ValueSpan switch
                                {
                                    "DBG" => OutputLineType.DEBUG_LOG,
                                    "WRN" => OutputLineType.WARNING_LOG,
                                    "ERR" => OutputLineType.ERROR_LOG,
                                    _ => OutputLineType.INFO_LOG
                                }
                            };
                            lastOutputType = outputLine.Type;
                            Output.Add(outputLine);
                        }
                        else
                        {
                            Output.Add(new OutputLine
                            {
                                Timestamp = "",
                                LogText = args.Data,
                                Type = lastOutputType
                            });
                        }
                    };
                    serverProcess.BeginOutputReadLine();
                }
                serverProcess.Exited += (_, _) =>
                {
                    onExited?.Invoke();
                };
            }
        }

        public static ServerProcess Start(string saveDir, Action onExited, bool isEmbedded) => new(saveDir, onExited, isEmbedded);

        /// <summary>
        ///     Tries to close the server gracefully with a timeout of 7 seconds. If it fails, returns false.
        /// </summary>
        public async Task<bool> CloseAsync()
        {
            using CancellationTokenSource ctsCloseTimeout = new(TimeSpan.FromSeconds(7));
            try
            {
                do
                {
                    if (!await SendCommandAsync("stop", ctsCloseTimeout.Token))
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

        public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            if (!IsRunning || string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            try
            {
                return await ipc.SendCommand(command, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignored
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
            ipc?.Dispose();
            serverProcess?.Dispose();
            serverProcess = null;
        }
    }
}

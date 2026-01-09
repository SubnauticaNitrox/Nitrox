using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Exceptions;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Serialization;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.Serialization;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
    public const string DEFAULT_SERVER_ICON_NAME = "servericon.png";
    private static readonly Dictionary<string, ServerEntry> entriesByDirectory = [];
    private static readonly Lock entriesByDirectoryLocker = new();

    private static readonly SubnauticaServerOptions serverDefaults = new();

    [ObservableProperty]
    private bool allowCommands = !serverDefaults.DisableConsole;

    [ObservableProperty]
    private bool allowKeepInventory = serverDefaults.KeepInventoryOnDeath;

    [ObservableProperty]
    private bool allowLanDiscovery = serverDefaults.LanDiscovery;

    [ObservableProperty]
    private bool allowPvP = serverDefaults.PvpEnabled;

    [ObservableProperty]
    private int autoSaveInterval = serverDefaults.SaveInterval / 1000;

    public Channel<string> CommandQueue = Channel.CreateUnbounded<string>();
    private CancellationTokenSource cts = new();

    [ObservableProperty]
    private SubnauticaGameMode gameMode = serverDefaults.GameMode;

    [ObservableProperty]
    private bool isEmbedded;

    [ObservableProperty]
    private bool isNewServer = true;

    [ObservableProperty]
    private bool isOnline;

    [ObservableProperty]
    private bool isServerClosing;

    [ObservableProperty]
    private DateTime lastAccessedTime = DateTime.Now;

    private int lastProcessId;

    [ObservableProperty]
    private int maxPlayers = serverDefaults.MaxConnections;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private Perms playerPermissions = serverDefaults.DefaultPlayerPerm;

    [ObservableProperty]
    private int players;

    [ObservableProperty]
    private int port = serverDefaults.ServerPort;

    [ObservableProperty]
    private bool portForward = serverDefaults.PortForward;

    [ObservableProperty]
    private string? seed;

    [ObservableProperty]
    private Bitmap? serverIcon;

    [ObservableProperty]
    private Version version = NitroxEnvironment.Version;

    internal ServerProcess? Process { get; private set; }
    public AvaloniaList<OutputLine> Output { get; } = [];

    /// <summary>
    ///     Gets the last process id known by this server entry.
    /// </summary>
    public int LastProcessId
    {
        get => ProcessId == 0 ? lastProcessId : lastProcessId = ProcessId;
    }

    public int ProcessId
    {
        get
        {
            int processId = Process?.Id ?? 0;
            if (processId > 0)
            {
                lastProcessId = processId;
            }
            return processId;
        }
    }

    /// <summary>
    ///     Should not be used directly unless for tests.
    /// </summary>
    internal ServerEntry()
    {
    }

    public static async Task<ServerEntry?> FromDirectoryAsync(string saveDir)
    {
        ServerEntry entry;
        lock (entriesByDirectoryLocker)
        {
            if (!entriesByDirectory.TryGetValue(saveDir, out entry))
            {
                entriesByDirectory[saveDir] = entry = new();
            }
        }
        await entry.RefreshFromDirectoryAsync(saveDir);
        return entry;
    }

    public static async Task<ServerEntry?> CreateNew(string saveDir, SubnauticaGameMode saveGameMode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveDir);

        Directory.CreateDirectory(saveDir);

        SubnauticaServerOptions config = NitroxConfig.Load<SubnauticaServerOptions>(saveDir);
        string fileEnding = config.SerializerMode switch
        {
            ServerSerializerMode.JSON => ServerJsonSerializer.FILE_ENDING,
            ServerSerializerMode.PROTOBUF => ServerProtoBufSerializer.FILE_ENDING,
            _ => throw new NotImplementedException()
        };

        await File.WriteAllTextAsync(Path.Combine(saveDir, $"Version{fileEnding}"), (string?)null);
        config.GameMode = saveGameMode;
        NitroxConfig.CreateFile(saveDir, config);

        return await FromDirectoryAsync(saveDir);
    }

    public Task<bool> RefreshFromProcessAsync(int processId)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return Task.FromResult(false);
        }
        if (Process?.Id != processId)
        {
            Process = ServerProcess.Start(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name), cts, false, processId);
        }
        if (Process is { IsRunning: true })
        {
            // Even though it wasn't started as embedded, using gRPC we can manage server as embedded.
            IsEmbedded = true; // This enables embedded server view in launcher.
            IsOnline = true;
        }
        return Task.FromResult(true);
    }

    public async Task<bool> RefreshFromDirectoryAsync(string saveDir)
    {
        if (!File.Exists(Path.Combine(saveDir, SerializableFileNameAttribute.GetFileName<SubnauticaServerOptions>())))
        {
            Log.Warn($"Tried loading invalid save directory at '{saveDir}'");
            return false;
        }

        Bitmap? icon = null;
        string serverIconPath = Path.Combine(saveDir, DEFAULT_SERVER_ICON_NAME);
        if (File.Exists(serverIconPath))
        {
            icon = new Bitmap(Path.Combine(saveDir, DEFAULT_SERVER_ICON_NAME));
        }

        SubnauticaServerOptions config = NitroxConfig.Load<SubnauticaServerOptions>(saveDir);
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

        Version serverVersion;
        using (FileStream stream = new(saveFileVersion, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            switch (config.SerializerMode)
            {
                case ServerSerializerMode.JSON:
                    SaveFileVersion versionModel;
                    try
                    {
                        versionModel = JsonSerializer.Deserialize<SaveFileVersion>(stream);
                    }
                    catch (Exception)
                    {
                        versionModel = new SaveFileVersion(NitroxEnvironment.Version);
                    }
                    serverVersion = versionModel.Version;
                    break;
                case ServerSerializerMode.PROTOBUF:
                    serverVersion = new ServerProtoBufSerializer(null).Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        string prevName = Name;
        Name = Path.GetFileName(saveDir);
        if (prevName != Name)
        {
            await ResetAsync();
        }
        ServerIcon = icon;
        Password = config.ServerPassword;
        Seed = config.Seed;
        GameMode = config.GameMode;
        PlayerPermissions = config.DefaultPlayerPerm;
        AutoSaveInterval = config.SaveInterval / 1000;
        MaxPlayers = config.MaxConnections;
        Port = config.ServerPort;
        PortForward = config.PortForward;
        AllowLanDiscovery = config.LanDiscovery;
        AllowCommands = !config.DisableConsole;
        AllowPvP = config.PvpEnabled;
        AllowKeepInventory = config.KeepInventoryOnDeath;
        IsNewServer = !File.Exists(Path.Combine(saveDir, $"PlayerData{fileEnding}"));
        Version = serverVersion;
        // TODO: Store default option "IsEmbedded" in launcher cfg, not server.cfg
        IsEmbedded = RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Force embedded on MacOS
        LastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(saveDir, $"PlayerData{fileEnding}"))
                                                     ?
                                                     // This file is affected by server saving
                                                     Path.Combine(saveDir, $"PlayerData{fileEnding}")
                                                     :
                                                     // If the above file doesn't exist (server was never ran), use the Version file instead
                                                     Path.Combine(saveDir, $"Version{fileEnding}"));
        return true;
    }

    public void Start(string savesDir, int existingProcessId = 0)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new Exception($"Server {nameof(Name)} not set");
        }
        if (!Directory.Exists(savesDir))
        {
            throw new DirectoryNotFoundException($"Directory '{savesDir}' not found");
        }
        if (Process?.IsRunning ?? false)
        {
            throw new DuplicateSingularApplicationException("Nitrox Server");
        }

        // Start server and add notify when server closed.
        Process = ServerProcess.Start(Path.Combine(savesDir, Name), cts, IsEmbedded, existingProcessId);

        Output.Clear();
        IsNewServer = false;
        IsOnline = true;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task StopAsync()
    {
        await cts.CancelAsync();
        // Ensure the server is dead before continuing. On Linux, if launcher process closes it could otherwise abruptly kill the embedded servers.
        using CancellationTokenSource waitProcessExitCts = new(TimeSpan.FromSeconds(20));
        try
        {
            while (ProcessEx.ProcessExists(GetServerExeName(), ex => ex.Id == LastProcessId))
            {
                await Task.Delay(200, waitProcessExitCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSaveFolder))]
    public void OpenSaveFolder()
    {
        System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name!),
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IsOnline) when LastProcessId > 0:
                WeakReferenceMessenger.Default.Send(new ServerStatusMessage(LastProcessId, IsOnline, Players));
                break;
        }
        base.OnPropertyChanged(e);
    }

    internal async Task ResetAsync()
    {
        if (!cts.IsCancellationRequested)
        {
            try
            {
                await cts.CancelAsync().WaitAsync(TimeSpan.FromSeconds(3));
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }
        cts = new();
        cts.Token.Register(async void () =>
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => IsServerClosing = true);
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    while (ProcessEx.ProcessExists(GetServerExeName(), ex => ex.Id == LastProcessId))
                    {
                        try
                        {
                            await CommandQueue.Writer.WriteAsync("quit");
                            CommandQueue.Writer.TryComplete();
                        }
                        catch (ChannelClosedException)
                        {
                            await Task.Delay(500);
                        }
                    }
                    CommandQueue = Channel.CreateUnbounded<string>();
                    IsOnline = false;
                    Output.Clear();
                });
                await Dispatcher.UIThread.InvokeAsync(() => IsServerClosing = false);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        });
    }

    private bool CanOpenSaveFolder() => !string.IsNullOrWhiteSpace(Name);

    internal class ServerProcess : IDisposable
    {
        private Process? serverProcess;
        public int Id => serverProcess?.Id ?? 0;
        public bool IsRunning => serverProcess != null;

        private ServerProcess(string saveDir, CancellationTokenSource cts, bool isEmbeddedMode = false, int processId = 0)
        {
            if (processId == 0)
            {
                string saveName = Path.GetFileName(saveDir);
                string launcherPath = NitroxUser.ExecutableRootPath;
                if (string.IsNullOrWhiteSpace(launcherPath))
                {
                    throw new Exception($"{nameof(launcherPath)} must be set");
                }

                string serverFile = Path.Combine(launcherPath, GetServerExeName());
                ProcessStartInfo startInfo = new(serverFile)
                {
                    WorkingDirectory = launcherPath,
                    ArgumentList =
                    {
                        "--save",
                        saveName
                    },
                    WindowStyle = isEmbeddedMode ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = isEmbeddedMode
                };
                // Assist server with finding launcher location.
                if (Directory.Exists(launcherPath))
                {
                    startInfo.EnvironmentVariables.Add(NitroxUser.LAUNCHER_PATH_ENV_KEY, launcherPath);
                }
                if (isEmbeddedMode)
                {
                    startInfo.ArgumentList.Add("--embedded");
                }
                Log.Info($"Starting server:{Environment.NewLine}File: {startInfo.FileName}{Environment.NewLine}Working directory: {startInfo.WorkingDirectory}{Environment.NewLine}Arguments: {string.Join(", ", startInfo.ArgumentList)}");

                serverProcess = System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                serverProcess = System.Diagnostics.Process.GetProcessById(processId);
            }
            cts.Token.Register(Dispose);
        }

        public static ServerProcess Start(string saveDir, CancellationTokenSource cts, bool isEmbedded, int processId) => new(saveDir, cts, isEmbedded, processId);

        public void Dispose()
        {
            serverProcess?.Dispose();
            serverProcess = null;
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Exceptions;
using Nitrox.Launcher.Models.Messages;
using Nitrox.Launcher.ViewModels;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Manager object for a server. Used to start/stop a server and change its settings.
/// </summary>
public partial class ServerEntry : ObservableObject
{
    // ServerEntry Variables
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
    private bool isNewServer = true;
    [ObservableProperty]
    private DateTime lastAccessedTime = DateTime.Now;

    private ServerProcess serverProcess;

    public ServerEntry()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ServerEntryPropertyChangedMessage(e.PropertyName));
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
            serverProcess = ServerProcess.Start(Name, () => Dispatcher.UIThread.InvokeAsync(Stop));
        }
        catch (Exception ex)
        {
            // TODO: Display these errors as warnings in UI:
            // - "An instance of the Nitrox server is already running, please close it to start another server."
            // - Any error returned from starting the server.
            Log.Error(ex);
            return;
        }

        IsNewServer = false;
        IsOnline = true;
    }

    [RelayCommand]
    public void Stop()
    {
        serverProcess?.Close();
        IsOnline = false;
    }

    private class ServerProcess : IDisposable
    {
        public string SaveName { get; init; }
        public bool IsRunning => !serverProcess?.HasExited ?? false;
        public string SaveDir => Path.Combine(ServersViewModel.SavesFolderDir, SaveName);
        private Process serverProcess;

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

        public void Close()
        {
            serverProcess?.CloseMainWindow();
            Dispose();
        }

        public void Dispose()
        {
            serverProcess?.Dispose();
            serverProcess = null;
        }

        public static ServerProcess Start(string saveName, Action onExited)
        {
            return new(saveName, onExited);
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Messages;
using Nitrox.Launcher.ViewModels;
using NitroxModel.DataStructures.GameLogic;
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
    
    
    // Server Variables
    public const string SERVER_EXECUTABLE = "NitroxServer-Subnautica.exe";
    public bool IsManagedByLauncher => IsEmbedded && IsServerRunning;
    public bool IsServerRunning => !serverProcess?.HasExited ?? false;
    public bool IsEmbedded { get; private set; }
    public event EventHandler<ServerStartEventArgs> ServerStarted;
    public event DataReceivedEventHandler ServerDataReceived;
    public event EventHandler ServerExited;

    private Process serverProcess;
    
    
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
        string saveDir = Path.Combine(ServersViewModel.SavesFolderDir, Name);
        bool standalone = true;
        
        try
        {
            if (IsServerRunning)
            {
                throw new Exception("An instance of Nitrox Server is already running");
            }
            
            string launcherDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string serverPath = Path.Combine(launcherDir, "server", SERVER_EXECUTABLE);
            ProcessStartInfo startInfo = new(serverPath)
            {
                WorkingDirectory = launcherDir
            };
            
            if (!standalone)
            {
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.CreateNoWindow = true;
            }
            else
            {
                startInfo.Verb = "open";
            }

            startInfo.Arguments = $@"""{saveDir}""";

            serverProcess = Process.Start(startInfo);
            if (serverProcess != null)
            {
                serverProcess.EnableRaisingEvents = true; // Required for 'Exited' event from process.

                if (!standalone)
                {
                    serverProcess.OutputDataReceived += ServerProcessOnOutputDataReceived;
                    serverProcess.BeginOutputReadLine();
                }

                serverProcess.Exited += (sender, args) => OnStopServer();
                OnStartServer(!standalone);
            }
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains("An instance of Nitrox Server is already running"))
            {
                //LauncherNotifier.Error("An instance of the Nitrox server is already running, please close it to start another server.");
                Debug.WriteLine("An instance of the Nitrox server is already running, please close it to start another server.");
            }
            else
            {
                //MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(ex.ToString());
            }
            return;
        }
        
        if (File.Exists(Path.Combine(saveDir, "WorldData.json")))
        {
            File.SetLastWriteTime(Path.Combine(saveDir, "WorldData.json"), DateTime.Now);
        }
        
        IsNewServer = false;
        IsOnline = true;
    }

    private void OnStartServer(bool embedded)
    {
        IsEmbedded = embedded;
        ServerStarted?.Invoke(serverProcess, new ServerStartEventArgs(embedded));
    }
    
    [RelayCommand]
    public void Stop()
    {
        if (IsEmbedded)
        {
            SendServerCommand("stop\n");
        }

        serverProcess?.Dispose();
        serverProcess = null;
        
        IsOnline = false;
    }
    
    private void OnStopServer()
    {
        IsEmbedded = false;
        ServerExited?.Invoke(serverProcess, EventArgs.Empty);
    }
    
    private void SendServerCommand(string inputText)
    {
        if (!IsServerRunning)
        {
            return;
        }

        try
        {
            serverProcess.StandardInput.WriteLine(inputText);
        }
        catch (Exception ex)
        {
            //Log.Error(ex);
        }
    }
    
    private void ServerProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        ServerDataReceived?.Invoke(sender, e);
    }
    
}

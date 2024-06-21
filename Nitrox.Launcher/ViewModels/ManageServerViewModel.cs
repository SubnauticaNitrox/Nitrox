using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class ManageServerViewModel : RoutableViewModelBase
{
    private readonly IDialogService dialogService;
    private readonly string savesFolderDir = KeyValueStore.Instance.GetValue("SavesFolderDir", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves"));
    private ServerEntry server;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAllowCommands;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAllowLanDiscovery;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private bool serverAutoPortForward;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(10, 86400, ErrorMessage = "Value must be between 10s and 24 hours (86400s).")]
    private int serverAutoSaveInterval;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private Perms serverDefaultPlayerPerm;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private NitroxGameMode serverGameMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private Bitmap serverIcon;

    private string serverIconDestinationDir;
    private string serverIconDir;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [Range(1, 1000)]
    [NotifyDataErrorInfo]
    private int serverMaxPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NotEndsWith(".")]
    [NitroxUniqueSaveName(true, nameof(OriginalServerName))]
    private string serverName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private string serverPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    private int serverPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(ushort.MinValue, ushort.MaxValue)]
    private int serverPort;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [NitroxWorldSeed]
    private string serverSeed;

    public static Array PlayerPerms => Enum.GetValues(typeof(Perms));
    public string OriginalServerName => Server?.Name;

    /// <summary>
    ///     When set, navigates to the <see cref="ManageServerView" />.
    /// </summary>
    public ServerEntry Server
    {
        get => server;
        private set
        {
            if (server != null)
            {
                server.PropertyChanged -= Server_PropertyChanged;
            }
            SetProperty(ref server, value);
            if (server != null)
            {
                server.PropertyChanged += Server_PropertyChanged;
            }
        }
    }

    private bool ServerIsOnline => Server.IsOnline;

    private string WorldFolderDirectory => Path.Combine(savesFolderDir, Server.Name);

    public ManageServerViewModel(IScreen screen, IDialogService dialogService) : base(screen)
    {
        this.dialogService = dialogService;
    }

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    public async Task StartServer()
    {
        // Check to ensure the Subnautica is not in legacy, skip if check fails
        try
        {
            if (NitroxUser.GamePlatform.Platform == Platform.STEAM)
            {
                string gameVersionFile = Path.Combine(NitroxUser.GamePath, "Subnautica_Data", "StreamingAssets", "SNUnmanagedData", "plastic_status.ignore");
                if (int.Parse(File.ReadAllText(gameVersionFile)) == 68598)
                {
                    await dialogService.ShowAsync<DialogBoxViewModel>(model =>
                    {
                        model.Title = "Legacy Game Detected";
                        model.Description = "Nitrox does not support the legacy version of Subnautica. Please update your game to the latest version to run the Nitrox server.";
                        model.ButtonOptions = ButtonOptions.Ok;
                    });
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while checking game version:");
            Log.Info("Skipping game version check...");
        }

        Server.Start();

        RestoreBackupCommand.NotifyCanExecuteChanged();
        DeleteServerCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task<bool> StopServerAsync()
    {
        if (!await Server.StopAsync())
        {
            return false;
        }

        RestoreBackupCommand.NotifyCanExecuteChanged();
        DeleteServerCommand.NotifyCanExecuteChanged();
        return true;
    }

    public void LoadFrom(ServerEntry serverEntry)
    {
        Server = serverEntry;

        ServerName = Server.Name;
        ServerIcon = Server.ServerIcon;
        ServerPassword = Server.Password;
        ServerGameMode = Server.GameMode;
        ServerSeed = Server.Seed;
        ServerDefaultPlayerPerm = Server.PlayerPermissions;
        ServerAutoSaveInterval = Server.AutoSaveInterval;
        ServerMaxPlayers = Server.MaxPlayers;
        ServerPlayers = Server.Players;
        ServerPort = Server.Port;
        ServerAutoPortForward = Server.AutoPortForward;
        ServerAllowLanDiscovery = Server.AllowLanDiscovery;
        ServerAllowCommands = Server.AllowCommands;
    }

    private bool HasChanges() => ServerName != Server.Name ||
                                 ServerIcon != Server.ServerIcon ||
                                 ServerPassword != Server.Password ||
                                 ServerGameMode != Server.GameMode ||
                                 ServerSeed != Server.Seed ||
                                 ServerDefaultPlayerPerm != Server.PlayerPermissions ||
                                 ServerAutoSaveInterval != Server.AutoSaveInterval ||
                                 ServerMaxPlayers != Server.MaxPlayers ||
                                 ServerPlayers != Server.Players ||
                                 ServerPort != Server.Port ||
                                 ServerAutoPortForward != Server.AutoPortForward ||
                                 ServerAllowLanDiscovery != Server.AllowLanDiscovery ||
                                 ServerAllowCommands != Server.AllowCommands;

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    private void Back()
    {
        HostScreen.Back();
    }

    private bool CanGoBackAndStartServer() => !HasChanges();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // If world name was changed, rename save folder to match it
        string newDir = Path.Combine(savesFolderDir, ServerName);
        if (WorldFolderDirectory != newDir)
        {
            // Windows, by default, ignores case when renaming folders. We circumvent this by changing the name to a random one, and then to the desired name.
            DirectoryInfo temp = Directory.CreateTempSubdirectory();
            temp.Delete();
            Directory.Move(WorldFolderDirectory, temp.FullName);
            Directory.Move(temp.FullName, newDir);
        }

        // Update the servericon.png file if needed
        if (Server.ServerIcon != ServerIcon && serverIconDir != null && serverIconDestinationDir != null)
        {
            File.Copy(serverIconDir, serverIconDestinationDir, true);
        }

        Server.Name = ServerName;
        Server.ServerIcon = ServerIcon;
        Server.Password = ServerPassword;
        Server.GameMode = ServerGameMode;
        Server.Seed = ServerSeed;
        Server.PlayerPermissions = ServerDefaultPlayerPerm;
        Server.AutoSaveInterval = ServerAutoSaveInterval;
        Server.MaxPlayers = ServerMaxPlayers;
        Server.Players = ServerPlayers;
        Server.Port = ServerPort;
        Server.AutoPortForward = ServerAutoPortForward;
        Server.AllowLanDiscovery = ServerAllowLanDiscovery;
        Server.AllowCommands = ServerAllowCommands;

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(WorldFolderDirectory);
        using (config.Update(WorldFolderDirectory))
        {
            config.SaveName = Server.Name;
            config.ServerPassword = Server.Password;
            if (Server.IsNewServer) { config.Seed = Server.Seed; }
            config.GameMode = Server.GameMode;
            config.DefaultPlayerPerm = Server.PlayerPermissions;
            config.SaveInterval = Server.AutoSaveInterval * 1000; // Convert seconds to milliseconds
            config.MaxConnections = Server.MaxPlayers;
            config.ServerPort = Server.Port;
            config.AutoPortForward = Server.AutoPortForward;
            config.LANDiscoveryEnabled = Server.AllowLanDiscovery;
            config.DisableConsole = !Server.AllowCommands;
        }

        Undo(); // Used to update the UI with corrected values (Trims and ToUppers)

        BackCommand.NotifyCanExecuteChanged();
        StartServerCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() => !HasErrors && !ServerIsOnline && HasChanges();

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        ServerName = Server.Name;
        ServerIcon = Server.ServerIcon;
        ServerPassword = Server.Password;
        ServerGameMode = Server.GameMode;
        ServerSeed = Server.Seed;
        ServerDefaultPlayerPerm = Server.PlayerPermissions;
        ServerAutoSaveInterval = Server.AutoSaveInterval;
        ServerMaxPlayers = Server.MaxPlayers;
        ServerPlayers = Server.Players;
        ServerPort = Server.Port;
        ServerAutoPortForward = Server.AutoPortForward;
        ServerAllowLanDiscovery = Server.AllowLanDiscovery;
        ServerAllowCommands = Server.AllowCommands;
    }

    private bool CanUndo() => !ServerIsOnline && HasChanges();

    [RelayCommand]
    private async Task ChangeServerIconAsync()
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select an image",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImagePng, FilePickerFileTypes.ImageJpg }
            });

            serverIconDir = files[0].TryGetLocalPath();
            serverIconDestinationDir = Path.Combine(WorldFolderDirectory, "servericon.png");

            ServerIcon = new(serverIconDir);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    [RelayCommand]
    private async Task OpenAdvancedSettings()
    {
        await dialogService.ShowAsync<ObjectPropertyEditorViewModel>(model =>
        {
            model.Title = $"Server '{ServerName}' config editor";
            model.OwnerObject = SubnauticaServerConfig.Load(WorldFolderDirectory);
            // TODO: Fix filter
            model.FieldAcceptFilter = p => !((string[])["password", "filename"]).Any(v => p.Name.Contains(v, StringComparison.OrdinalIgnoreCase));
        });
    }

    [RelayCommand]
    private void OpenWorldFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = WorldFolderDirectory,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackupAndDeleteServer))]
    private void RestoreBackup()
    {
        // TODO: Open Restore Backup Popup
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackupAndDeleteServer))]
    private async Task DeleteServer()
    {
        DialogBoxViewModel modalViewModel = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Description = $"Are you sure you want to delete the server '{ServerName}'?";
            model.DescriptionFontSize = 24;
            model.DescriptionFontWeight = FontWeight.ExtraBold;
            model.ButtonOptions = ButtonOptions.YesNo;
        });
        if (!modalViewModel)
        {
            return;
        }

        Directory.Delete(WorldFolderDirectory, true);
        WeakReferenceMessenger.Default.Send(new SaveDeletedMessage(ServerName));
        HostScreen.Back();
    }

    private bool CanRestoreBackupAndDeleteServer() => !ServerIsOnline;

    private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerEntry.IsOnline))
        {
            OnPropertyChanged(nameof(ServerIsOnline));
        }
    }
}

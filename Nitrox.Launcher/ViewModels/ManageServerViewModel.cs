using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Server;
using ReactiveUI;
using Config = NitroxModel.Serialization.SubnauticaServerConfig;

namespace Nitrox.Launcher.ViewModels;

public partial class ManageServerViewModel : RoutableViewModelBase
{
    private readonly string[] advancedSettingsDeniedFields =
    [
        "password", "filename", nameof(Config.ServerPort), nameof(Config.MaxConnections), nameof(Config.AutoPortForward), nameof(Config.SaveName), nameof(Config.SaveInterval), nameof(Config.Seed), nameof(Config.GameMode), nameof(Config.DisableConsole),
        nameof(Config.LANDiscoveryEnabled), nameof(Config.DefaultPlayerPerm)
    ];

    private readonly IDialogService dialogService;
    private readonly IKeyValueStore keyValueStore;
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
    [NitroxUniqueSaveName(nameof(SavesFolderDir), true, nameof(OriginalServerName))]
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

    private string SaveFolderDirectory => Path.Combine(SavesFolderDir, Server.Name);
    private string SavesFolderDir => keyValueStore.GetSavesFolderDir();

    public ManageServerViewModel(IScreen screen, IDialogService dialogService, IKeyValueStore keyValueStore) : base(screen)
    {
        this.dialogService = dialogService;
        this.keyValueStore = keyValueStore;
    }

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    public async Task StartServer()
    {
        if (await GameInspect.IsOutdatedGameAndNotify(NitroxUser.GamePath, dialogService))
        {
            return;
        }

        Server.Start(keyValueStore.GetSavesFolderDir());
    }

    [RelayCommand]
    public async Task<bool> StopServerAsync()
    {
        if (!await Server.StopAsync())
        {
            return false;
        }

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
    private void Back() => HostScreen.Back();

    private bool CanGoBackAndStartServer() => !HasChanges();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // If world name was changed, rename save folder to match it
        string newDir = Path.Combine(SavesFolderDir, ServerName);
        if (SaveFolderDirectory != newDir)
        {
            // Windows, by default, ignores case when renaming folders. We circumvent this by changing the name to a random one, and then to the desired name.
            DirectoryInfo temp = Directory.CreateTempSubdirectory();
            temp.Delete();
            Directory.Move(SaveFolderDirectory, temp.FullName);
            Directory.Move(temp.FullName, newDir);
        }

        // Update the servericon.png file if needed
        if (Server.ServerIcon != ServerIcon && serverIconDir != null)
        {
            File.Copy(serverIconDir, Path.Combine(newDir, "servericon.png"), true);
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

        Config config = Config.Load(SaveFolderDirectory);
        using (config.Update(SaveFolderDirectory))
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
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("All Images + Icons")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.ico" },
                        AppleUniformTypeIdentifiers = new[] { "public.image" },
                        MimeTypes = new[] { "image/*" }
                    }
                }
            });
            string newIconFile = files.FirstOrDefault()?.TryGetLocalPath();
            if (newIconFile == null || !File.Exists(newIconFile))
            {
                return;
            }

            serverIconDir = newIconFile;
            ServerIcon = new Bitmap(serverIconDir);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    [RelayCommand]
    private async Task ShowAdvancedSettings()
    {
        ObjectPropertyEditorViewModel result = await dialogService.ShowAsync<ObjectPropertyEditorViewModel>(model =>
        {
            model.Title = $"Server '{ServerName}' config editor";
            model.FieldAcceptFilter = p => !advancedSettingsDeniedFields.Any(v => p.Name.Contains(v, StringComparison.OrdinalIgnoreCase));
            model.OwnerObject = Config.Load(SaveFolderDirectory);
        });
        if (result && result.OwnerObject is Config config)
        {
            config.Serialize(SaveFolderDirectory);
        }
        LoadFrom(server);
    }

    [RelayCommand]
    private void OpenWorldFolder() =>
        Process.Start(new ProcessStartInfo
        {
            FileName = SaveFolderDirectory,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();

    [RelayCommand(CanExecute = nameof(CanRestoreBackupAndDeleteServer))]
    private async Task RestoreBackup()
    {
        BackupRestoreViewModel result = await dialogService.ShowAsync<BackupRestoreViewModel>(model =>
        {
            model.Title = $"Restore a Backup for '{ServerName}'";
            model.SaveFolderDirectory = SaveFolderDirectory;
        });

        if (result)
        {
            string backupFile = result.SelectedBackup.BackupFileName;
            try
            {
                if (!File.Exists(backupFile))
                {
                    throw new FileNotFoundException("Selected backup file not found.", backupFile);
                }

                ZipFile.ExtractToDirectory(backupFile, SaveFolderDirectory, true);
                server.RefreshFromDirectory(SaveFolderDirectory);
                LoadFrom(server);
                LauncherNotifier.Success("Backup restored successfully.");
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex, "Error while restoring backup");
            }
        }
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

        try
        {
            Directory.Delete(SaveFolderDirectory, true);
            WeakReferenceMessenger.Default.Send(new SaveDeletedMessage(ServerName));
            HostScreen.Back();
        }
        catch (Exception ex)
        {
            await dialogService.ShowErrorAsync(ex, $"Error while deleting world \"{ServerName}\"");
        }
    }

    private bool CanRestoreBackupAndDeleteServer() => !ServerIsOnline;

    private void Server_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerEntry.IsOnline))
        {
            OnPropertyChanged(nameof(ServerIsOnline));
            RestoreBackupCommand.NotifyCanExecuteChanged();
            DeleteServerCommand.NotifyCanExecuteChanged();
        }
    }
}

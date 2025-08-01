﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Server;
using Config = NitroxModel.Serialization.SubnauticaServerConfig;

namespace Nitrox.Launcher.ViewModels;

internal partial class ManageServerViewModel : RoutableViewModelBase
{
    private readonly string[] advancedSettingsDeniedFields =
    [
        "password", "filename", nameof(Config.ServerPort), nameof(Config.MaxConnections), nameof(Config.AutoPortForward), nameof(Config.SaveInterval), nameof(Config.Seed), nameof(Config.GameMode), nameof(Config.DisableConsole),
        nameof(Config.LANDiscoveryEnabled), nameof(Config.DefaultPlayerPerm), nameof(Config.IsEmbedded), nameof(Config.KeepInventoryOnDeath), nameof(Config.PvPEnabled), nameof(Config.SerializerMode)
    ];

    private readonly DialogService dialogService;
    private readonly IKeyValueStore keyValueStore;
    private readonly ServerService serverService;
    private readonly StorageService storageService;

    [ObservableProperty]
    private ServerEntry? server;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private bool serverAllowCommands;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private bool serverAllowKeepInventory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private bool serverAllowLanDiscovery;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private bool serverAllowPvP;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private bool serverAutoPortForward;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(10, 86400, ErrorMessage = "Value must be between 10s and 24 hours (86400s).")]
    private int serverAutoSaveInterval;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private Perms serverDefaultPlayerPerm;

    [ObservableProperty]
    private bool serverEmbedded = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private NitroxGameMode serverGameMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private Bitmap? serverIcon;

    private string? serverIconDir;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreBackupCommand), nameof(DeleteServerCommand))]
    private bool serverIsOnline;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    [Range(1, 1000)]
    [NotifyDataErrorInfo]
    private int serverMaxPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [FileName]
    [NotEndsWith(".")]
    [NitroxUniqueSaveName(nameof(SavesFolderDir), true, nameof(OriginalServerName))]
    private string? serverName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private string? serverPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    private int serverPlayers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [Range(ushort.MinValue, ushort.MaxValue)]
    private int serverPort;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(UndoCommand), nameof(BackCommand), nameof(RestoreBackupCommand), nameof(StartServerCommand))]
    [NotifyDataErrorInfo]
    [NitroxWorldSeed]
    private string? serverSeed;

    public static Array PlayerPerms => Enum.GetValues(typeof(Perms));
    public string? OriginalServerName => Server?.Name;

    private string SaveFolderDirectory => Path.Combine(SavesFolderDir, Server?.Name ?? throw new Exception($"{nameof(Server)} is not set"));
    private string SavesFolderDir => keyValueStore.GetSavesFolderDir();

    public ManageServerViewModel(DialogService dialogService, StorageService storageService, IKeyValueStore keyValueStore, ServerService serverService)
    {
        this.dialogService = dialogService;
        this.storageService = storageService;
        this.keyValueStore = keyValueStore;
        this.serverService = serverService;

        this.RegisterMessageListener<ServerStatusMessage, ManageServerViewModel>((status, vm) =>
        {
            if (vm.server?.Process?.Id != status.ProcessId)
            {
                return;
            }
            vm.ServerIsOnline = status.IsOnline;
            vm.ServerPlayers = status.PlayerCount;
        });
    }

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    public async Task StartServerAsync() => await serverService.StartServerAsync(Server!);

    [RelayCommand]
    public async Task StopServerAsync() => await Server!.StopAsync();

    public void LoadFrom(ServerEntry serverEntry)
    {
        Server = serverEntry;

        ServerIsOnline = Server.IsOnline;
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
        ServerAllowPvP = Server.AllowPvP;
        ServerAllowKeepInventory = Server.AllowKeepInventory;
        ServerEmbedded = Server.IsEmbedded || RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Force embedded on macOS
    }

    private bool HasChanges() => Server != null &&
                                 (ServerName != Server.Name ||
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
                                  ServerAllowCommands != Server.AllowCommands ||
                                  ServerAllowPvP != Server.AllowPvP ||
                                  ServerAllowKeepInventory != Server.AllowKeepInventory);

    [RelayCommand(CanExecute = nameof(CanGoBackAndStartServer))]
    private void Back() => ChangeViewToPrevious<ServersViewModel>();

    private bool CanGoBackAndStartServer() => !HasChanges();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // If world name was changed, rename save folder to match it
        string newPath = Path.Combine(SavesFolderDir, ServerName);
        if (SaveFolderDirectory != newPath)
        {
            // Windows, by default, ignores case when renaming folders. We circumvent this by changing the name to a random one, and then to the desired name.
            // OS tmp directory is not used because on Linux this causes cross-link error, see https://github.com/dotnet/runtime/issues/31149
            string tempSavePath = Path.Combine(SavesFolderDir, $"{Guid.NewGuid():N}_{ServerName[..Math.Min(ServerName.Length, 10)]}");
            Directory.Move(SaveFolderDirectory, tempSavePath);
            Directory.Move(tempSavePath, newPath);
        }

        // Update the servericon.png file if needed
        if (Server.ServerIcon != ServerIcon && serverIconDir != null)
        {
            File.Copy(serverIconDir, Path.Combine(newPath, "servericon.png"), true);
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
        Server.AllowPvP = ServerAllowPvP;
        Server.AllowKeepInventory = ServerAllowKeepInventory;

        Config config = Config.Load(SaveFolderDirectory);
        using (config.Update(SaveFolderDirectory))
        {
            config.ServerPassword = Server.Password;
            if (Server.IsNewServer) { config.Seed = Server.Seed; }
            config.GameMode = Server.GameMode;
            config.DefaultPlayerPerm = Server.PlayerPermissions;
            config.SaveInterval = (int)TimeSpan.FromSeconds(Server.AutoSaveInterval).TotalMilliseconds;
            config.MaxConnections = Server.MaxPlayers;
            config.ServerPort = Server.Port;
            config.AutoPortForward = Server.AutoPortForward;
            config.LANDiscoveryEnabled = Server.AllowLanDiscovery;
            config.DisableConsole = !Server.AllowCommands;
            config.PvPEnabled = Server.AllowPvP;
            config.KeepInventoryOnDeath = Server.AllowKeepInventory;
        }

        Undo(); // Used to update the UI with corrected values (Trims and ToUppers)

        BackCommand.NotifyCanExecuteChanged();
        StartServerCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RestoreBackupCommand.NotifyCanExecuteChanged();
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
        ServerAllowPvP = Server.AllowPvP;
        ServerAllowKeepInventory = Server.AllowKeepInventory;
    }

    private bool CanUndo() => !ServerIsOnline && HasChanges();

    [RelayCommand]
    private async Task ChangeServerIconAsync()
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await storageService.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select an image",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("All Images + Icons")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.ico"],
                        AppleUniformTypeIdentifiers = ["public.image"],
                        MimeTypes = ["image/*"]
                    }
                ]
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
            model.DisableButtons = Server.IsOnline;
        });
        if (result && result!.OwnerObject is Config config)
        {
            config.Serialize(SaveFolderDirectory);
        }
        LoadFrom(Server);
    }

    [RelayCommand]
    private void OpenWorldFolder() =>
        Process.Start(new ProcessStartInfo
        {
            FileName = SaveFolderDirectory,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();

    [RelayCommand(CanExecute = nameof(CanRestoreBackup))]
    private async Task RestoreBackup()
    {
        BackupRestoreViewModel result = await dialogService.ShowAsync<BackupRestoreViewModel>(model =>
        {
            model.Title = $"Restore a Backup for '{ServerName}'";
            model.SaveFolderDirectory = SaveFolderDirectory;
        });

        if (result)
        {
            string backupFile = result!.SelectedBackup?.BackupFilePath;
            try
            {
                if (!File.Exists(backupFile))
                {
                    throw new FileNotFoundException("Selected backup file not found.", backupFile);
                }

                bool isEmbedded = ServerEmbedded;
                foreach (string file in Directory.GetFiles(SaveFolderDirectory, "*"))
                {
                    File.Delete(file);
                }
                ZipFile.ExtractToDirectory(backupFile, SaveFolderDirectory, true);
                Server!.RefreshFromDirectory(SaveFolderDirectory);
                LoadFrom(Server);
                ServerEmbedded = isEmbedded; // Preserve the original IsEmbedded value
                LauncherNotifier.Success("Backup restored successfully.");
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex, "Error while restoring backup");
            }
        }
    }

    private bool CanRestoreBackup() => !ServerIsOnline && !HasChanges();

    [RelayCommand(CanExecute = nameof(CanDeleteServer))]
    private async Task DeleteServerAsync()
    {
        await CoreDeleteServerAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteServer))]
    private async Task ForceDeleteServerAsync()
    {
        await CoreDeleteServerAsync(true);
    }

    private async Task CoreDeleteServerAsync(bool force = false)
    {
        if (!force)
        {
            DialogBoxViewModel modal = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
            {
                model.Title = $"Are you sure you want to delete the server '{ServerName}'?";
                model.ButtonOptions = ButtonOptions.YesNo;
            });
            if (!modal)
            {
                return;
            }
        }

        try
        {
            Directory.Delete(SaveFolderDirectory, true);
            WeakReferenceMessenger.Default.Send(new SaveDeletedMessage(ServerName));
            ChangeViewToPrevious();
        }
        catch (Exception ex)
        {
            await dialogService.ShowErrorAsync(ex, $"Error while deleting world \"{ServerName}\"");
        }
    }

    private bool CanDeleteServer() => !ServerIsOnline;

    partial void OnServerEmbeddedChanged(bool value)
    {
        Server.IsEmbedded = value || RuntimeInformation.IsOSPlatform(OSPlatform.OSX); // Force embedded on macOS

        Config config = Config.Load(SaveFolderDirectory);
        using (config.Update(SaveFolderDirectory))
        {
            config.IsEmbedded = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.ViewModels;

internal partial class UpdatesViewModel(NitroxWebsiteApiService nitroxWebsiteApi, DialogService dialogService, ServerService serverService, Func<Window> mainWindowProvider, BackupService backupService) : RoutableViewModelBase
{
    private readonly DialogService dialogService = dialogService;
    private readonly ServerService serverService = serverService;
    private readonly Func<Window> mainWindowProvider = mainWindowProvider;
    private readonly NitroxWebsiteApiService nitroxWebsiteApi = nitroxWebsiteApi;
    private readonly BackupService backupService = backupService;
    private CancellationTokenSource? downloadCts;

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private string? downloadStatus;

    [ObservableProperty]
    private bool newUpdateAvailable;

    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];

    [ObservableProperty]
    private string? officialVersion;

    [ObservableProperty]
    private bool usingOfficialVersion;

    [ObservableProperty]
    private AvaloniaList<BackupInfo> availableBackups = [];

    [ObservableProperty]
    private string? version;

    public async Task<bool> IsNitroxUpdateAvailableAsync()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            Version latestVersion = (await nitroxWebsiteApi.GetNitroxLatestVersionAsync())?.Version ?? new Version(0, 0);

            NewUpdateAvailable = latestVersion > currentVersion;
            if (NewUpdateAvailable)
            {
                string versionMessage = $"A new version of the mod ({latestVersion}) is available.";
                Log.Info(versionMessage);
                LauncherNotifier.Warning(versionMessage);
            }
            Version = currentVersion.ToString();
            OfficialVersion = latestVersion.ToString();
            UsingOfficialVersion = NitroxEnvironment.IsReleaseMode && latestVersion >= currentVersion;
        }
        catch
        {
            NewUpdateAvailable = false;
            UsingOfficialVersion = NitroxEnvironment.IsReleaseMode;
        }

        return NewUpdateAvailable || !UsingOfficialVersion;
    }

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                NitroxChangelogs.Clear();
                NitroxChangelogs.AddRange(await nitroxWebsiteApi.GetChangeLogsAsync(cancellationToken)! ?? []);

                // Load available backups
                RefreshBackups();
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    LauncherNotifier.Error("Failed to fetch Nitrox changelogs");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display Nitrox changelogs");
            }
        });
    }

    private static async Task<string> CreateUpdaterScriptAsync(string sourcePath, string destinationPath, string tempDir)
    {
        // Safety check: ensure destination path is rooted to prevent accidental file deletion
        if (!Path.IsPathRooted(destinationPath))
        {
            throw new ArgumentException("Destination path must be an absolute path", nameof(destinationPath));
        }

        string scriptPath;
        string scriptContent;
        string launcherFilePath = Path.Combine(destinationPath, Path.GetFileName(NitroxUser.ExecutableFilePath) ?? throw new Exception("Failed to get executable file name"));

        if (OperatingSystem.IsWindows())
        {
            scriptPath = Path.Combine(tempDir, "update.bat");
            scriptContent = $"""
                             @echo off
                             echo Waiting for Nitrox Launcher to close...
                             :waitloop
                             tasklist /FI "IMAGENAME eq Nitrox.Launcher.exe" 2>NUL | find /I /N "Nitrox.Launcher.exe">NUL
                             if "%ERRORLEVEL%"=="0" (
                                 timeout /t 1 /nobreak >nul
                                 goto waitloop
                             )
                             echo Cleaning old installation...
                             for %%F in ("{destinationPath}\*.dll") do del /Q "%%F" 2>nul
                             for %%F in ("{destinationPath}\*.exe") do del /Q "%%F" 2>nul
                             for %%F in ("{destinationPath}\*.json") do del /Q "%%F" 2>nul
                             for %%F in ("{destinationPath}\*.config") do del /Q "%%F" 2>nul
                             for %%F in ("{destinationPath}\*.txt") do del /Q "%%F" 2>nul
                             if exist "{destinationPath}\lib" rmdir /S /Q "{destinationPath}\lib" 2>nul
                             if exist "{destinationPath}\runtimes" rmdir /S /Q "{destinationPath}\runtimes" 2>nul
                             if exist "{destinationPath}\Resources" rmdir /S /Q "{destinationPath}\Resources" 2>nul
                             echo Installing update...
                             xcopy /E /Y /I "{sourcePath}\*" "{destinationPath}\"
                             if errorlevel 1 (
                                 echo Update failed! Press any key to exit...
                                 pause >nul
                                 exit /b 1
                             )
                             echo Starting Nitrox Launcher...
                             start "" "{launcherFilePath}"
                             exit
                             """;
        }
        else
        {
            scriptPath = Path.Combine(tempDir, "update.sh");
            scriptContent = $"""
                             #!/bin/bash
                             echo "Waiting for Nitrox Launcher to close..."
                             while pgrep -x "Nitrox.Launcher" > /dev/null; do
                                 sleep 1
                             done
                             echo "Cleaning old installation..."
                             rm -f "{destinationPath}"/*.dll 2>/dev/null
                             rm -f "{destinationPath}"/*.exe 2>/dev/null
                             rm -f "{destinationPath}"/*.json 2>/dev/null
                             rm -f "{destinationPath}"/*.config 2>/dev/null
                             rm -f "{destinationPath}"/*.txt 2>/dev/null
                             rm -rf "{destinationPath}/lib" 2>/dev/null
                             rm -rf "{destinationPath}/runtimes" 2>/dev/null
                             rm -rf "{destinationPath}/Resources" 2>/dev/null
                             echo "Installing update..."
                             cp -rf "{sourcePath}/"* "{destinationPath}/"
                             echo "Starting Nitrox Launcher..."
                             chmod +x "{launcherFilePath}"
                             nohup "{launcherFilePath}" >/dev/null 2>&1 &
                             """;
        }

        await File.WriteAllTextAsync(scriptPath, scriptContent);

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(scriptPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return scriptPath;
    }

    [RelayCommand(CanExecute = nameof(CanDownloadUpdate), AllowConcurrentExecutions = false)]
    private async Task DownloadUpdate()
    {
        if (await nitroxWebsiteApi.GetNitroxLatestVersionAsync() is not { CurrentPlatformInfo: {} downloadInfo } latestRelease)
        {
            LauncherNotifier.Error("No update information available for your platform. Please refresh and try again.");
            return;
        }
        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = $"Download and install Nitrox {latestRelease.Version} ({downloadInfo.FileSizeMegaBytes:F1} MB)?";
            if (NitroxEnvironment.IsReleaseMode)
            {
                model.Description = "The will overwrite your current Nitrox installation and restart Nitrox after the update is complete.\nPlease check if this update is compatible with your current save file before continuing.";
            }
            else
            {
                model.Description = "Development build detected. Please use git pull to update your local repository.";
            }
            model.Description += "\n\nDo you want to continue?";
            model.ButtonOptions = ButtonOptions.YesNo;
        });
        if (!confirmResult)
        {
            return;
        }

        DownloadProgress = 0;
        DownloadStatus = "Creating backup...";

        // Create backup before updating
        string? backupPath = await backupService.CreateBackupAsync(
            includeSaves: true,
            progress: new Progress<(int Progress, string Status)>(p =>
            {
                DownloadProgress = p.Progress * 0.1; // Backup is 10% of total progress
                DownloadStatus = p.Status;
            })
        );

        if (backupPath == null)
        {
            DialogBoxViewModel backupFailedResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
            {
                model.Title = "Backup failed";
                model.Description = "Failed to create a backup of your current installation. Do you want to continue with the update anyway?";
                model.ButtonOptions = ButtonOptions.YesNo;
            });
            if (!backupFailedResult)
            {
                DownloadProgress = 0;
                DownloadStatus = null;
                return;
            }
        }
        else
        {
            LauncherNotifier.Success($"Backup created: {Path.GetFileName(backupPath)}");
        }

        DownloadStatus = "Starting download...";
        using (downloadCts = new CancellationTokenSource())
        {
            try
            {
                string currentDir = NitroxUser.LauncherPath ?? AppDomain.CurrentDomain.BaseDirectory;
                string tempDir = Path.Combine(Path.GetTempPath(), "NitroxUpdate");
                string zipPath = Path.Combine(tempDir, $"Nitrox_{latestRelease.Version}.zip");
                string extractPath = Path.Combine(tempDir, "extract");

                // Clean up any previous update attempt
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);

                // Download the update
                DownloadStatus = "Downloading...";
                using (HttpFileService.FileDownloader? downloader = await nitroxWebsiteApi.GetLatestNitroxAsync(downloadCts.Token))
                {
                    if (downloader == null)
                    {
                        return;
                    }
                    await foreach (long bytesRead in downloader.DownloadToFileInStepsAsync(zipPath))
                    {
                        if (downloader.SizeFromServer < 1)
                        {
                            continue;
                        }
                        DownloadProgress = (double)bytesRead / downloader.SizeFromServer * 100;
                        DownloadStatus = $"Downloading... {bytesRead / 1024.0 / 1024.0:F1} / {downloader.SizeFromServer / 1024.0 / 1024.0:F1} MB";
                    }
                }

                // Verify MD5 hash if provided
                if (!string.IsNullOrEmpty(downloadInfo.Md5Hash))
                {
                    DownloadStatus = "Verifying download...";
                    DownloadProgress = 100;
                    string downloadedHash = await Hashing.ComputeMd5HashAsync(zipPath);
                    if (!string.Equals(downloadedHash, downloadInfo.Md5Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException($"Download verification failed. Expected hash: {downloadInfo.Md5Hash}, got: {downloadedHash}");
                    }
                }

                // Extract the update
                DownloadStatus = "Extracting...";
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                // Find the Nitrox folder inside the extracted content
                string nitroxFolder = extractPath;
                string[] subDirs = Directory.GetDirectories(extractPath);
                if (subDirs.Length == 1)
                {
                    nitroxFolder = subDirs[0];
                }

                // Create the updater batch script
                string scriptFilePath = await CreateUpdaterScriptAsync(nitroxFolder, currentDir, tempDir);

                DownloadStatus = "Installing update...";
                LauncherNotifier.Success("Update downloaded successfully. Restarting to apply update...");

                // Start the updater script and exit
                using Process? script = ProcessEx.StartProcessDetached(new ProcessStartInfo
                {
                    FileName = scriptFilePath,
                    CreateNoWindow = true
                });
                mainWindowProvider().CloseByCode();
            }
            catch (OperationCanceledException)
            {
                DownloadProgress = 0;
                DownloadStatus = "Download cancelled";
                LauncherNotifier.Info("Update download cancelled.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to download or install update");
                DownloadProgress = 0;
                DownloadStatus = "Download failed";
                await dialogService.ShowErrorAsync(ex, "Update Failed", "Failed to download or install the update.");
            }
        }
    }

    private bool CanDownloadUpdate() => !CanCancelDownload() && serverService.Servers.All(s => !s.IsOnline);

    [RelayCommand(CanExecute = nameof(CanCancelDownload))]
    private void CancelDownload()
    {
        downloadCts?.Cancel();
    }

    private bool CanCancelDownload() => downloadCts is { IsCancellationRequested: false };

    [RelayCommand]
    private void RefreshBackups()
    {
        AvailableBackups.Clear();
        AvailableBackups.AddRange(backupService.GetAvailableBackups());
    }

    [RelayCommand]
    private async Task DeleteBackup(BackupInfo? backup)
    {
        if (backup == null)
        {
            return;
        }

        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = "Delete backup?";
            model.Description = $"Are you sure you want to delete the backup '{backup.FileName}'?\nThis action cannot be undone.";
            model.ButtonOptions = ButtonOptions.YesNo;
        });

        if (confirmResult && backupService.DeleteBackup(backup.FilePath))
        {
            AvailableBackups.Remove(backup);
            LauncherNotifier.Success("Backup deleted");
        }
    }

    [RelayCommand]
    private async Task RestoreBackup(BackupInfo? backup)
    {
        if (backup == null)
        {
            return;
        }

        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = "Restore backup?";
            model.Description = $"This will restore Nitrox to version {backup.Version} and overwrite your current installation.";
            if (backup.IncludesSaves)
            {
                model.Description += "\n\nThis backup includes save files which will also be restored, potentially overwriting your current saves.";
            }
            model.Description += "\n\nThe launcher will close and restart after the restore is complete.\n\nDo you want to continue?";
            model.ButtonOptions = ButtonOptions.YesNo;
        });

        if (!confirmResult)
        {
            return;
        }

        string? scriptPath = await backupService.CreateRestoreScriptAsync(backup.FilePath);
        if (scriptPath == null)
        {
            LauncherNotifier.Error("Failed to create restore script");
            return;
        }

        LauncherNotifier.Success("Restoring backup... The launcher will restart.");

        // Start the restore script and exit
        using Process? script = ProcessEx.StartProcessDetached(new ProcessStartInfo
        {
            FileName = scriptPath,
            CreateNoWindow = true
        });
        mainWindowProvider().CloseByCode();
    }

    [RelayCommand]
    private void OpenBackupsFolder()
    {
        string backupsDir = BackupService.BackupsDirectory;
        Directory.CreateDirectory(backupsDir);

        try
        {
            OpenDirectory(backupsDir);
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error($"Failed to open backups folder: {ex.Message}");
        }
    }
}

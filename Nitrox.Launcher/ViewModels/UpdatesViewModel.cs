using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
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

internal partial class UpdatesViewModel : RoutableViewModelBase
{
    private readonly NitroxWebsiteApiService nitroxWebsiteApi;
    private readonly DialogService dialogService;

    private NitroxWebsiteApiService.NitroxRelease? latestRelease;
    private NitroxWebsiteApiService.ArchitectureInfo? downloadInfo;
    private CancellationTokenSource? downloadCancellationTokenSource;

    [ObservableProperty]
    private bool newUpdateAvailable;

    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];

    [ObservableProperty]
    private string? officialVersion;

    [ObservableProperty]
    private bool usingOfficialVersion;

    [ObservableProperty]
    private string? version;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadUpdateCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCommand))]
    private bool isDownloading;

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private string? downloadStatus;

    public UpdatesViewModel(NitroxWebsiteApiService nitroxWebsiteApi, DialogService dialogService)
    {
        this.nitroxWebsiteApi = nitroxWebsiteApi;
        this.dialogService = dialogService;
    }

    public async Task<bool> IsNitroxUpdateAvailableAsync()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            latestRelease = await nitroxWebsiteApi.GetNitroxLatestVersionAsync();
            Version latestVersion = latestRelease?.Version ?? new Version(0, 0);
            downloadInfo = latestRelease?.GetCurrentPlatformDownload();

            NewUpdateAvailable = latestVersion > currentVersion;
            UsingOfficialVersion = NitroxEnvironment.IsReleaseMode && latestVersion >= currentVersion;

            if (NewUpdateAvailable)
            {
                string versionMessage = $"A new version of the mod ({latestVersion}) is available.";
                Log.Info(versionMessage);
                LauncherNotifier.Warning(versionMessage);
            }

            Version = currentVersion.ToString();
            OfficialVersion = latestVersion.ToString();
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

    [RelayCommand(CanExecute = nameof(CanDownloadUpdate))]
    private async Task DownloadUpdate()
    {
        if (latestRelease == null || downloadInfo == null)
        {
            LauncherNotifier.Error("No update information available for your platform. Please refresh and try again.");
            return;
        }

        if (!NitroxEnvironment.IsReleaseMode)
        {
            LauncherNotifier.Info("Development build detected. Please use git pull to update your local repository.");
            return;
        }

        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = "Download and Install Update";
            model.Description = $"This will download Nitrox {latestRelease.Version} ({downloadInfo.FileSizeMegaBytes:F1} MB) and install it.\n\nThe launcher will restart after the update is complete.\n\nDo you want to continue?";
            model.ButtonOptions = ButtonOptions.YesNo;
        });

        if (!confirmResult)
        {
            return;
        }

        IsDownloading = true;
        DownloadProgress = 0;
        DownloadStatus = "Starting download...";
        downloadCancellationTokenSource = new CancellationTokenSource();

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
            Progress<(long bytesRead, long? totalBytes)> progress = new(p =>
            {
                if (p.totalBytes.HasValue)
                {
                    DownloadProgress = (double)p.bytesRead / p.totalBytes.Value * 100;
                    DownloadStatus = $"Downloading... {p.bytesRead / 1024.0 / 1024.0:F1} / {p.totalBytes.Value / 1024.0 / 1024.0:F1} MB";
                }
            });
            await nitroxWebsiteApi.DownloadFileAsync(downloadInfo.DownloadUrl, zipPath, progress, downloadCancellationTokenSource.Token);

            if (downloadCancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
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
            string updaterScript = CreateUpdaterScript(nitroxFolder, currentDir, tempDir);

            DownloadStatus = "Installing update...";
            LauncherNotifier.Success("Update downloaded successfully. Restarting to apply update...");

            // Start the updater script and exit
            ProcessEx.Start(updaterScript, createWindow: false);

            // Give the script a moment to start, then exit
            await Task.Delay(500);
            Environment.Exit(0);
        }
        catch (OperationCanceledException)
        {
            DownloadStatus = "Download cancelled";
            LauncherNotifier.Info("Update download cancelled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download or install update");
            DownloadStatus = "Download failed";
            await dialogService.ShowErrorAsync(ex, "Update Failed", "Failed to download or install the update.");
        }
        finally
        {
            IsDownloading = false;
            downloadCancellationTokenSource?.Dispose();
            downloadCancellationTokenSource = null;
        }
    }

    private bool CanDownloadUpdate() => !IsDownloading;

    [RelayCommand(CanExecute = nameof(CanCancelDownload))]
    private void CancelDownload()
    {
        downloadCancellationTokenSource?.Cancel();
    }

    private bool CanCancelDownload() => IsDownloading;

    private static string CreateUpdaterScript(string sourcePath, string destinationPath, string tempDir)
    {
        // Safety check: ensure destination path is rooted to prevent accidental file deletion
        if (!Path.IsPathRooted(destinationPath))
        {
            throw new ArgumentException("Destination path must be an absolute path", nameof(destinationPath));
        }

        string scriptPath;
        string scriptContent;
        string launcherExe = Path.Combine(destinationPath, "Nitrox.Launcher.exe");

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
                start "" "{launcherExe}"
                exit
                """;
        }
        else
        {
            string launcherPath = Path.Combine(destinationPath, "Nitrox.Launcher");
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
                chmod +x "{launcherPath}"
                nohup "{launcherPath}" >/dev/null 2>&1 &
                """;
        }

        File.WriteAllText(scriptPath, scriptContent);

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(scriptPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return scriptPath;
    }
}

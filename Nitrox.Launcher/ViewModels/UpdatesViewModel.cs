using System;
using System.Diagnostics;
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
    private readonly DialogService dialogService;
    private readonly NitroxWebsiteApiService nitroxWebsiteApi;
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
    private string? version;

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
        string launcherExe = Path.Combine(destinationPath, Path.GetFileName(NitroxUser.ExecutableFilePath) ?? throw new Exception("Failed to get executable file name"));

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
        if (!NitroxEnvironment.IsReleaseMode)
        {
            LauncherNotifier.Info("Development build detected. Please use git pull to update your local repository.");
            return;
        }
        if (await nitroxWebsiteApi.GetNitroxLatestVersionAsync() is not { CurrentPlatformInfo: {} downloadInfo } latestRelease)
        {
            LauncherNotifier.Error("No update information available for your platform. Please refresh and try again.");
            return;
        }
        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = $"Download and install Nitrox {latestRelease.Version} ({downloadInfo.FileSizeMegaBytes:F1} MB)?";
            model.Description = "The will overwrite your current Nitrox installation and restart Nitrox after the update is complete.\nPlease check if this update is compatible with your current save file before continuing.\n\nDo you want to continue?";
            model.ButtonOptions = ButtonOptions.YesNo;
        });
        if (!confirmResult)
        {
            return;
        }

        DownloadProgress = 0;
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
                Environment.Exit(0);
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

    private bool CanDownloadUpdate() => !CanCancelDownload();

    [RelayCommand(CanExecute = nameof(CanCancelDownload))]
    private void CancelDownload()
    {
        downloadCts?.Cancel();
    }

    private bool CanCancelDownload() => downloadCts is { IsCancellationRequested: false };
}

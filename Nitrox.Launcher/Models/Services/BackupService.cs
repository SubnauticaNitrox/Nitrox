using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Service for creating and restoring backups of Nitrox installations and save files.
/// </summary>
public class BackupService(IKeyValueStore keyValueStore)
{
    private readonly IKeyValueStore keyValueStore = keyValueStore;

    public static string BackupsDirectory => Path.Combine(NitroxUser.AppDataPath, "backups");

    /// <summary>
    ///     Creates a backup of the current Nitrox installation and optionally save files.
    /// </summary>
    /// <param name="includeSaves">Whether to include save files in the backup.</param>
    /// <param name="progress">Progress reporter (0-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the created backup file, or null if backup failed.</returns>
    public async Task<string?> CreateBackupAsync(bool includeSaves = true, IProgress<(int Progress, string Status)>? progress = null, CancellationToken cancellationToken = default)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupName = $"nitrox_backup_{NitroxEnvironment.Version}_{timestamp}";
        string backupFilePath = Path.Combine(BackupsDirectory, $"{backupName}.zip");

        try
        {
            Directory.CreateDirectory(BackupsDirectory);

            string? launcherPath = NitroxUser.LauncherPath;
            if (string.IsNullOrEmpty(launcherPath) || !Directory.Exists(launcherPath))
            {
                Log.Error("Cannot create backup: Launcher path not found");
                return null;
            }

            progress?.Report((0, "Creating backup..."));

            await using FileStream zipStream = new(backupFilePath, FileMode.Create);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

            // Backup installation files
            progress?.Report((10, "Backing up installation files..."));
            await AddDirectoryToArchiveAsync(archive, launcherPath, "installation", cancellationToken);

            // Backup save files if requested
            if (includeSaves)
            {
                progress?.Report((50, "Backing up save files..."));
                string savesDir = keyValueStore.GetSavesFolderDir();
                if (Directory.Exists(savesDir))
                {
                    await AddDirectoryToArchiveAsync(archive, savesDir, "saves", cancellationToken);
                }
            }

            // Add metadata file
            progress?.Report((90, "Finalizing backup..."));
            ZipArchiveEntry metadataEntry = archive.CreateEntry("backup_info.json");
            await using (Stream entryStream = metadataEntry.Open())
            {
                BackupMetadata metadata = new()
                {
                    NitroxVersion = NitroxEnvironment.Version.ToString(),
                    BackupDate = DateTime.Now,
                    IncludesSaves = includeSaves,
                    InstallationPath = launcherPath,
                    SavesPath = keyValueStore.GetSavesFolderDir()
                };
                await JsonSerializer.SerializeAsync(entryStream, metadata, JsonSerializerOptions.Default, cancellationToken);
            }

            progress?.Report((100, "Backup complete"));
            Log.Info($"Backup created: {backupFilePath}");
            return backupFilePath;
        }
        catch (OperationCanceledException)
        {
            // Clean up partial backup
            TryDeleteFile(backupFilePath);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create backup");
            // Clean up partial backup
            TryDeleteFile(backupFilePath);
            return null;
        }
    }

    /// <summary>
    ///     Gets a list of available backups.
    /// </summary>
    public IEnumerable<BackupInfo> GetAvailableBackups()
    {
        if (!Directory.Exists(BackupsDirectory))
        {
            yield break;
        }

        foreach (string file in Directory.EnumerateFiles(BackupsDirectory, "nitrox_backup_*.zip").OrderByDescending(f => f))
        {
            BackupInfo? info = GetBackupInfo(file);
            if (info != null)
            {
                yield return info;
            }
        }
    }

    /// <summary>
    ///     Gets information about a specific backup file.
    /// </summary>
    public BackupInfo? GetBackupInfo(string backupPath)
    {
        try
        {
            FileInfo fileInfo = new(backupPath);
            string fileName = Path.GetFileNameWithoutExtension(backupPath);

            // Try to read metadata from JSON file inside the archive
            string version = "Unknown";
            DateTime? date = null;
            bool includesSaves = false;

            using (ZipArchive archive = ZipFile.OpenRead(backupPath))
            {
                ZipArchiveEntry? metadataEntry = archive.GetEntry("backup_info.json");
                if (metadataEntry != null)
                {
                    using Stream stream = metadataEntry.Open();
                    BackupMetadata? metadata = JsonSerializer.Deserialize<BackupMetadata>(stream);
                    if (metadata != null)
                    {
                        version = metadata.NitroxVersion;
                        date = metadata.BackupDate;
                        includesSaves = metadata.IncludesSaves;
                    }
                }
                else
                {
                    // Fallback: parse version and date from filename for older backups
                    // Format: nitrox_backup_1.8.0.1_2024-01-15_12-30-45
                    string[] parts = fileName.Split('_');
                    version = parts.Length > 2 ? parts[2] : "Unknown";

                    if (parts.Length > 4 && DateTime.TryParse($"{parts[3]} {parts[4].Replace('-', ':')}", out DateTime parsedDate))
                    {
                        date = parsedDate;
                    }

                    includesSaves = archive.Entries.Any(e => e.FullName.StartsWith("saves/", StringComparison.OrdinalIgnoreCase));
                }
            }

            return new BackupInfo
            {
                FilePath = backupPath,
                FileName = fileInfo.Name,
                Version = version,
                CreatedAt = date ?? fileInfo.CreationTime,
                SizeBytes = fileInfo.Length,
                IncludesSaves = includesSaves
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to read backup info: {backupPath}");
            return null;
        }
    }

    /// <summary>
    ///     Deletes a backup file.
    /// </summary>
    public bool DeleteBackup(string backupPath)
    {
        try
        {
            if (TryDeleteFile(backupPath))
            {
                Log.Info($"Deleted backup: {backupPath}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to delete backup: {backupPath}");
            return false;
        }
    }

    /// <summary>
    ///     Creates a restore script that will apply the backup after the launcher closes.
    /// </summary>
    /// <param name="backupPath">Path to the backup zip file.</param>
    /// <returns>Path to the restore script, or null if creation failed.</returns>
    public async Task<string?> CreateRestoreScriptAsync(string backupPath)
    {
        try
        {
            string? launcherPath = NitroxUser.LauncherPath;
            if (string.IsNullOrEmpty(launcherPath))
            {
                Log.Error("Cannot restore backup: Launcher path not found");
                return null;
            }

            // Safety check: ensure paths are rooted to prevent accidental file deletion
            if (!Path.IsPathRooted(launcherPath))
            {
                throw new ArgumentException("Launcher path must be an absolute path", nameof(launcherPath));
            }

            string savesDir = keyValueStore.GetSavesFolderDir();
            if (!Path.IsPathRooted(savesDir))
            {
                throw new ArgumentException("Saves directory must be an absolute path", nameof(savesDir));
            }

            string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxRestore {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            Directory.CreateDirectory(tempDir);

            string extractPath = Path.Combine(tempDir, "extract");
            string launcherFilePath = Path.Combine(launcherPath, Path.GetFileName(NitroxUser.ExecutableFilePath) ?? "Nitrox.Launcher.exe");

            string scriptPath;
            string scriptContent;

            if (OperatingSystem.IsWindows())
            {
                scriptPath = Path.Combine(tempDir, "restore.bat");
                scriptContent = $"""
                                 @echo off
                                 echo Waiting for Nitrox Launcher to close...
                                 :waitloop
                                 tasklist /FI "IMAGENAME eq Nitrox.Launcher.exe" 2>NUL | find /I /N "Nitrox.Launcher.exe">NUL
                                 if "%ERRORLEVEL%"=="0" (
                                     timeout /t 1 /nobreak >nul
                                     goto waitloop
                                 )
                                 echo Extracting backup...
                                 powershell -Command "Expand-Archive -Path '{backupPath}' -DestinationPath '{extractPath}' -Force"
                                 if errorlevel 1 (
                                     echo Failed to extract backup! Press any key to exit...
                                     pause >nul
                                     exit /b 1
                                 )
                                 echo Restoring installation files...
                                 if exist "{extractPath}\installation" (
                                     for %%F in ("{launcherPath}\*.dll") do del /Q "%%F" 2>nul
                                     for %%F in ("{launcherPath}\*.exe") do del /Q "%%F" 2>nul
                                     for %%F in ("{launcherPath}\*.json") do del /Q "%%F" 2>nul
                                     for %%F in ("{launcherPath}\*.config") do del /Q "%%F" 2>nul
                                     for %%F in ("{launcherPath}\*.txt") do del /Q "%%F" 2>nul
                                     if exist "{launcherPath}\lib" rmdir /S /Q "{launcherPath}\lib" 2>nul
                                     if exist "{launcherPath}\runtimes" rmdir /S /Q "{launcherPath}\runtimes" 2>nul
                                     if exist "{launcherPath}\Resources" rmdir /S /Q "{launcherPath}\Resources" 2>nul
                                     xcopy /E /Y /I "{extractPath}\installation\*" "{launcherPath}\"
                                 )
                                 echo Restoring save files...
                                 if exist "{extractPath}\saves" (
                                     xcopy /E /Y /I "{extractPath}\saves\*" "{savesDir}\"
                                 )
                                 echo Cleaning up...
                                 rmdir /S /Q "{extractPath}" 2>nul
                                 echo Restore complete! Starting Nitrox Launcher...
                                 start "" "{launcherFilePath}"
                                 exit
                                 """;
            }
            else
            {
                scriptPath = Path.Combine(tempDir, "restore.sh");
                scriptContent = $"""
                                 #!/bin/bash
                                 echo "Waiting for Nitrox Launcher to close..."
                                 while pgrep -x "Nitrox.Launcher" > /dev/null; do
                                     sleep 1
                                 done
                                 echo "Extracting backup..."
                                 unzip -o "{backupPath}" -d "{extractPath}"
                                 if [ $? -ne 0 ]; then
                                     echo "Failed to extract backup!"
                                     exit 1
                                 fi
                                 echo "Restoring installation files..."
                                 if [ -d "{extractPath}/installation" ]; then
                                     rm -f "{launcherPath}"/*.dll 2>/dev/null
                                     rm -f "{launcherPath}"/*.exe 2>/dev/null
                                     rm -f "{launcherPath}"/*.json 2>/dev/null
                                     rm -f "{launcherPath}"/*.config 2>/dev/null
                                     rm -f "{launcherPath}"/*.txt 2>/dev/null
                                     rm -rf "{launcherPath}/lib" 2>/dev/null
                                     rm -rf "{launcherPath}/runtimes" 2>/dev/null
                                     rm -rf "{launcherPath}/Resources" 2>/dev/null
                                     cp -rf "{extractPath}/installation/"* "{launcherPath}/"
                                 fi
                                 echo "Restoring save files..."
                                 if [ -d "{extractPath}/saves" ]; then
                                     cp -rf "{extractPath}/saves/"* "{savesDir}/"
                                 fi
                                 echo "Cleaning up..."
                                 rm -rf "{extractPath}" 2>/dev/null
                                 echo "Restore complete! Starting Nitrox Launcher..."
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
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create restore script");
            return null;
        }
    }

    private static async Task AddDirectoryToArchiveAsync(ZipArchive archive, string sourceDir, string entryPrefix, CancellationToken cancellationToken)
    {
        foreach (string file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath = Path.GetRelativePath(sourceDir, file);
            string entryName = Path.Combine(entryPrefix, relativePath).Replace('\\', '/');

            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            await using FileStream fileStream = File.OpenRead(file);
            await using Stream entryStream = entry.Open();
            await fileStream.CopyToAsync(entryStream, cancellationToken);
        }
    }
}

public class BackupInfo
{
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required string Version { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required long SizeBytes { get; init; }
    public required bool IncludesSaves { get; init; }

    public string SizeDisplay => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} B",
        < 1024 * 1024 => $"{SizeBytes / 1024.0:F1} KB",
        _ => $"{SizeBytes / 1024.0 / 1024.0:F1} MB"
    };
}

public class BackupMetadata
{
    public string NitroxVersion { get; set; } = "";
    public DateTime BackupDate { get; set; }
    public bool IncludesSaves { get; set; }
    public string InstallationPath { get; set; } = "";
    public string SavesPath { get; set; } = "";
}

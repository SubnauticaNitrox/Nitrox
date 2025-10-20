using System.Diagnostics;
using System.IO;
using System.Linq;
using Nitrox.Model.Constants;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class SaveService(Func<WorldService> worldServiceProvider, IOptions<SubnauticaServerOptions> options, IOptions<ServerStartOptions> startOptions, ILogger<SaveService> logger) : BackgroundService, IHostedLifecycleService
{
    private readonly Func<WorldService> worldServiceProvider = worldServiceProvider;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<SaveService> logger = logger;
    private readonly AsyncBarrier saveBarrier = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await saveBarrier.WaitForSignalAsync(stoppingToken);

            string savePath = startOptions.Value.GetServerSavePath();
            if (!worldServiceProvider().Save(savePath))
            {
                continue;
            }
            ExecutePostSaveCommand();
            BackUp(savePath);
        }
    }

    public void QueueSave()
    {
        saveBarrier.Signal();
    }

    private void ExecutePostSaveCommand()
    {
        string postSaveCommandPath = options.Value.PostSaveCommandPath;
        if (!string.IsNullOrWhiteSpace(postSaveCommandPath))
        {
            try
            {
                // Call external tool for backups, etc
                if (File.Exists(postSaveCommandPath))
                {
                    using Process process = Process.Start(new ProcessStartInfo
                    {
                        FileName = postSaveCommandPath,
                        Verb = "open"
                    });
                    logger.ZLogInformation($"Post-save command completed successfully: {postSaveCommandPath}");
                }
                else
                {
                    logger.ZLogError($"Post-save file does not exist: {postSaveCommandPath}");
                }
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Post-save command failed");
            }
        }
    }

    private void BackUp(string saveDir)
    {
        if (options.Value.MaxBackups < 1)
        {
            logger.ZLogInformation($"No backup was made (\"{nameof(options.Value.MaxBackups)}\" is equal to 0)");
            return;
        }
        string backupDir = startOptions.Value.GetServerSaveBackupsPath();
        string tempOutDir = Path.Combine(backupDir, $"Backup - {DateTime.Now.ToString(PersistanceConstants.BACKUP_DATE_TIME_FORMAT)}");
        Directory.CreateDirectory(backupDir);

        try
        {
            // Prepare backup location
            Directory.CreateDirectory(tempOutDir);
            string newZipFile = $"{tempOutDir}.zip";
            if (File.Exists(newZipFile))
            {
                File.Delete(newZipFile);
            }
            foreach (string file in Directory.GetFiles(saveDir))
            {
                File.Copy(file, Path.Combine(tempOutDir, Path.GetFileName(file)));
            }

            FileSystem.Instance.ZipFilesInDirectory(tempOutDir, newZipFile);
            Directory.Delete(tempOutDir, true);
            logger.ZLogInformation($"World backed up");

            // Prune old backups
            FileInfo[] backups = Directory.EnumerateFiles(backupDir)
                                          .Select(f => new FileInfo(f))
                                          .Where(f => f is { Extension: ".zip" } info && info.Name.Contains("Backup - "))
                                          .OrderBy(f => File.GetCreationTime(f.FullName))
                                          .ToArray();
            if (backups.Length > options.Value.MaxBackups)
            {
                int numBackupsToDelete = backups.Length - Math.Max(1, options.Value.MaxBackups);
                for (int i = 0; i < numBackupsToDelete; i++)
                {
                    backups[i].Delete();
                }
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error while backing up world");
            if (Directory.Exists(tempOutDir))
            {
                Directory.Delete(tempOutDir, true); // Delete the outZip folder that is sometimes left
            }
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        QueueSave();
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        QueueSave();
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

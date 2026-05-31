using System.Diagnostics;
using System.IO;
using System.Linq;
using Nitrox.Model.Constants;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Serialization.World;
using Nitrox.Server.Subnautica.Services.Core;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class SaveService(Func<WorldService> worldServiceProvider, ISaveState.Trigger saveStateTrigger, IOptions<SubnauticaServerOptions> options, IOptions<ServerStartOptions> startOptions, ILogger<SaveService> logger)
    : QueuingBackgroundService<SaveService.ServiceAction>, IHibernate
{
    private readonly ILogger<SaveService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ISaveState.Trigger saveStateTrigger = saveStateTrigger;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly Func<WorldService> worldServiceProvider = worldServiceProvider;

    protected override async Task ExecuteQueuedActionAsync(ServiceAction action, CancellationToken stoppingToken)
    {
        switch (action)
        {
            case ServiceAction.SAVE:
                string savePath = startOptions.Value.GetServerSavePath();
                if (!worldServiceProvider().Save(savePath))
                {
                    return;
                }
                await saveStateTrigger.InvokeAsync(new ISaveState.Args(savePath));
                ExecutePostSaveCommand();
                BackUp(savePath);
                break;
        }
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

    async Task IEvent<IHibernate.SleepArgs>.OnEventAsync(IHibernate.SleepArgs args) => await QueueActionAsync(ServiceAction.SAVE);

    Task IEvent<IHibernate.WakeArgs>.OnEventAsync(IHibernate.WakeArgs args) => Task.CompletedTask;

    internal enum ServiceAction
    {
        SAVE
    }
}

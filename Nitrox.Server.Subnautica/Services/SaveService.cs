using System.Diagnostics;
using System.IO;
using System.Linq;
using Nitrox.Model.Constants;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class SaveService(WorldService service, IOptions<SubnauticaServerOptions> options, IOptions<ServerStartOptions> startOptions, ILogger<SaveService> logger) : BackgroundService, IHostedLifecycleService
{
    private readonly WorldService service = service;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<SaveService> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     // TODO: SEMAPHORE
        // }

        //     public void BackUp()
        // {
        //     if (!IsRunning)
        //     {
        //         return;
        //     }

        //     Save();

        //     worldPersistence.BackUp(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name));
        // }

        // public bool Start(string saveName, CancellationTokenSource ct)
        // {
        //     Validate.NotNull(ct);
        //     if (ct.IsCancellationRequested)
        //     {
        //         return false;
        //     }
        //     if (!server.Start(ct.Token))
        //     {
        //         return false;
        //     }
        //     Name = saveName;
        //     serverCancelSource = ct;
        //     IsRunning = true;

        //     Save(); // Ensures save files exist when server is running
        //     if (!serverConfig.DisableAutoBackup)
        //     {
        //         worldPersistence.BackUp(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), saveName));
        //     }

        //     try
        //     {
        //         if (serverConfig.CreateFullEntityCache)
        //         {
        //             Log.Info("Starting to load all batches up front.");
        //             Log.Info("This can take up to several minutes and you can't join until it's completed.");
        //             Log.Info($"{entityRegistry.GetAllEntities().Count} entities already cached");
        //             if (entityRegistry.GetAllEntities().Count < 504732)
        //             {
        //                 worldEntityManager.LoadAllUnspawnedEntities(serverCancelSource.Token);

        //                 Log.Info("Saving newly cached entities.");
        //                 Save();
        //             }
        //             Log.Info("All batches have now been loaded.");
        //         }
        //     }
        //     catch (OperationCanceledException ex)
        //     {
        //         Log.Warn($"Server start was cancelled by user:{Environment.NewLine}{ex.Message}");
        //         return false;
        //     }
        //     Log.Info($"Server is listening on port {Port} UDP");
        //     Log.Info($"Using {serverConfig.SerializerMode} as save file serializer");
        //     Log.InfoSensitive("Server Password: {password}", string.IsNullOrEmpty(serverConfig.ServerPassword) ? "None. Public Server." : serverConfig.ServerPassword);
        //     Log.InfoSensitive("Admin Password: {password}", serverConfig.AdminPassword);
        //     Log.Info($"Autosave: {(serverConfig.DisableAutoSave ? "DISABLED" : $"ENABLED ({serverConfig.SaveInterval / 60000} min)")}");
        //     Log.Info($"Autobackup: {(serverConfig.DisableAutoBackup || serverConfig.MaxBackups == 0 ? "DISABLED" : "ENABLED")} (Max Backups: {serverConfig.MaxBackups})");
        //     Log.Info($"Loaded save\n{GetSaveSummary()}");

        //     PauseServer();

        //     return true;
        // }
    }

    public void Save()
    {
        bool savedSuccessfully = service.Save(startOptions.Value.GetServerSavePath());
        string postSaveCommandPath = options.Value.PostSaveCommandPath;
        if (savedSuccessfully && !string.IsNullOrWhiteSpace(postSaveCommandPath))
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

    public void BackUp(string saveDir)
    {
        if (options.Value.MaxBackups < 1)
        {
            Log.Info($"No backup was made (\"{nameof(options.Value.MaxBackups)}\" is equal to 0)");
            return;
        }
        string backupDir = Path.Combine(saveDir, "Backups");
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
            Log.Info("World backed up");

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
            Log.Error(ex, "Error while backing up world");
            if (Directory.Exists(tempOutDir))
            {
                Directory.Delete(tempOutDir, true); // Delete the outZip folder that is sometimes left
            }
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        Save();
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

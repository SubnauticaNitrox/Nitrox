using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;
using Timer = System.Timers.Timer;

namespace NitroxServer;

public class Server
{
    private readonly Communication.NitroxServer server;
    private readonly WorldPersistence worldPersistence;
    private readonly SubnauticaServerConfig serverConfig;
    private readonly Timer saveTimer;
    private readonly World world;
    private readonly WorldEntityManager worldEntityManager;
    private readonly EntityRegistry entityRegistry;

    private CancellationTokenSource serverCancelSource;

    public static Server Instance { get; private set; }

    public bool IsRunning { get; private set; }

    public bool IsSaving { get; private set; }

    public string Name { get; private set; } = "My World";
    public int Port => serverConfig?.ServerPort ?? -1;

    public Server(WorldPersistence worldPersistence, World world, SubnauticaServerConfig serverConfig, Communication.NitroxServer server, WorldEntityManager worldEntityManager, EntityRegistry entityRegistry)
    {
        this.worldPersistence = worldPersistence;
        this.serverConfig = serverConfig;
        this.server = server;
        this.world = world;
        this.worldEntityManager = worldEntityManager;
        this.entityRegistry = entityRegistry;

        Instance = this;

        saveTimer = new Timer();
        saveTimer.Interval = serverConfig.SaveInterval;
        saveTimer.AutoReset = true;
        saveTimer.Elapsed += delegate
        {
            if (!serverConfig.DisableAutoBackup && serverConfig.MaxBackups != 0)
            {
                BackUp();
            }
            else
            {
                Save();
            }
        };
    }

    public string GetSaveSummary(Perms viewerPerms = Perms.CONSOLE)
    {
        // TODO: Extend summary with more useful save file data
        // Note for later additions: order these lines by their length
        StringBuilder builder = new("\n");
        if (viewerPerms is Perms.CONSOLE)
        {
            builder.AppendLine($" - Save location: {Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name)}");
        }
        builder.AppendLine($"""
         - Aurora's state: {world.StoryManager.GetAuroraStateSummary()}
         - Current time: day {world.TimeKeeper.Day} ({Math.Floor(world.TimeKeeper.ElapsedSeconds)}s)
         - Scheduled goals stored: {world.GameData.StoryGoals.ScheduledGoals.Count}
         - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}
         - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}
         - World gamemode: {serverConfig.GameMode}
         - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}
         - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}
        """);

        return builder.ToString();
    }

    // TODO : Remove this method once server hosting/loading happens as a service (see '.NET Generic Host' on msdn)
    public static SubnauticaServerConfig CreateOrLoadConfig()
    {
        string? saveDir = null;
        if (GetSaveName(Environment.GetCommandLineArgs()) is { } saveName)
        {
            saveDir = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), saveName);
        }

        if (Directory.Exists(saveDir))
        {
            return SubnauticaServerConfig.Load(saveDir);
        }

        // Check if there are any save files
        List<ServerListing> saves = GetSaves();

        if (saves.Count > 0)
        {
            // Get last save file used
            string lastSaveAccessed = saves[0].SaveDir;

            if (saves.Count > 1)
            {
                for (int i = 1; i < saves.Count; i++)
                {
                    if (File.GetLastWriteTime(Path.Combine(saves[i].SaveDir, $"WorldData{ServerProtoBufSerializer.FILE_ENDING}")) > File.GetLastWriteTime(lastSaveAccessed))
                    {
                        lastSaveAccessed = saves[i].SaveDir;
                    }
                    else if (File.GetLastWriteTime(Path.Combine(saves[i].SaveDir, $"WorldData{ServerJsonSerializer.FILE_ENDING}")) > File.GetLastWriteTime(lastSaveAccessed))
                    {
                        lastSaveAccessed = saves[i].SaveDir;
                    }
                }
            }

            saveDir = lastSaveAccessed;
        }
        else
        {
            // Create new save file
            Log.Debug("No save file was found, creating a new one...");
            saveDir = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), "My World");
            Directory.CreateDirectory(saveDir);
        }

        return SubnauticaServerConfig.Load(saveDir);
    }

    public void Save()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;

        bool savedSuccessfully = worldPersistence.Save(world, Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name));
        if (savedSuccessfully && !string.IsNullOrWhiteSpace(serverConfig.PostSaveCommandPath))
        {
            try
            {
                // Call external tool for backups, etc
                if (File.Exists(serverConfig.PostSaveCommandPath))
                {
                    using Process process = Process.Start(serverConfig.PostSaveCommandPath);
                    Log.Info($"Post-save command completed successfully: {serverConfig.PostSaveCommandPath}");
                }
                else
                {
                    Log.Error($"Post-save file does not exist: {serverConfig.PostSaveCommandPath}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Post-save command failed");
            }
        }
        IsSaving = false;
    }

    public bool Start(string saveName, CancellationTokenSource ct)
    {
        Debug.Assert(serverCancelSource == null);

        Validate.NotNull(ct);
        if (ct.IsCancellationRequested)
        {
            return false;
        }
        if (!server.Start(ct.Token))
        {
            return false;
        }
        Name = saveName;
        serverCancelSource = ct;
        IsRunning = true;

        if (!serverConfig.DisableAutoBackup)
        {
            worldPersistence.BackUp(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), saveName));
        }

        try
        {
            if (serverConfig.CreateFullEntityCache)
            {
                Log.Info("Starting to load all batches up front.");
                Log.Info("This can take up to several minutes and you can't join until it's completed.");
                Log.Info($"{entityRegistry.GetAllEntities().Count} entities already cached");
                if (entityRegistry.GetAllEntities().Count < 504732)
                {
                    worldEntityManager.LoadAllUnspawnedEntities(serverCancelSource.Token);

                    Log.Info("Saving newly cached entities.");
                    Save();
                }
                Log.Info("All batches have now been loaded.");
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.Warn($"Server start was cancelled by user:{Environment.NewLine}{ex.Message}");
            return false;
        }

        LogHowToConnectAsync().ContinueWithHandleError(ex => Log.Warn($"Failed to show how to connect: {ex.GetFirstNonAggregateMessage()}"));
        Log.Info($"Server is listening on port {Port} UDP");
        Log.Info($"Using {serverConfig.SerializerMode} as save file serializer");
        Log.InfoSensitive("Server Password: {password}", string.IsNullOrEmpty(serverConfig.ServerPassword) ? "None. Public Server." : serverConfig.ServerPassword);
        Log.InfoSensitive("Admin Password: {password}", serverConfig.AdminPassword);
        Log.Info($"Autosave: {(serverConfig.DisableAutoSave ? "DISABLED" : $"ENABLED ({serverConfig.SaveInterval / 60000} min)")}");
        Log.Info($"Autobackup: {(serverConfig.DisableAutoBackup || serverConfig.MaxBackups == 0 ? "DISABLED" : "ENABLED")} (Max Backups: {serverConfig.MaxBackups})");
        Log.Info($"Loaded save\n{GetSaveSummary()}");

        PauseServer();

        return true;
    }

    public void Stop(bool shouldSave = true)
    {
        if (!IsRunning)
        {
            return;
        }
        IsRunning = false;

        try
        {
            serverCancelSource.Cancel();
        }
        catch
        {
            // ignored
        }

        Log.Info("Nitrox Server Stopping...");
        DisablePeriodicSaving();

        if (shouldSave)
        {
            Save();
        }

        server.Stop();
        Log.Info("Nitrox Server Stopped");
    }

    public void BackUp()
    {
        if (!IsRunning)
        {
            return;
        }

        Save();

        worldPersistence.BackUp(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), Name));
    }

    private async Task LogHowToConnectAsync()
    {
        Task<IPAddress> localIp = Task.Run(NetHelper.GetLanIp);
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
        Task<IPAddress> hamachiIp = Task.Run(NetHelper.GetHamachiIp);

        List<string> options = ["127.0.0.1 - You (Local)"];
        if (await wanIp != null)
        {
            options.Add("{ip:l} - Friends on another internet network (Port Forwarding)");
        }
        if (await hamachiIp != null)
        {
            options.Add($"{hamachiIp.Result} - Friends using Hamachi (VPN)");
        }
        // LAN IP could be null if all Ethernet/Wi-Fi interfaces are disabled.
        if (await localIp != null)
        {
            options.Add($"{localIp.Result} - Friends on same internet network (LAN)");
        }

        Log.InfoSensitive($"Use IP to connect:{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", options)}", wanIp.Result);
    }

    public void StopAndWait(bool shouldSave = true)
    {
        Stop(shouldSave);
        Log.Info("Press enter to continue");
        Console.Read();
    }

    public void EnablePeriodicSaving()
    {
        saveTimer.Start();
    }

    public void DisablePeriodicSaving()
    {
        saveTimer.Stop();
    }

    public void PauseServer()
    {
        DisablePeriodicSaving();
        world.TimeKeeper.StopCounting();
        Log.Info("Server has paused, waiting for players to connect");
    }

    public void ResumeServer()
    {
        if (!serverConfig.DisableAutoSave)
        {
            EnablePeriodicSaving();
        }
        world.TimeKeeper.StartCounting();
        Log.Info("Server has resumed");
    }

    private static List<ServerListing> GetSaves()
    {
        try
        {
            Directory.CreateDirectory(KeyValueStore.Instance.GetSavesFolderDir());

            List<ServerListing> saves = [];
            foreach (string saveDir in Directory.EnumerateDirectories(KeyValueStore.Instance.GetSavesFolderDir()))
            {
                try
                {
                    ServerListing entryFromDir = ServerListing.Validate(saveDir);
                    if (entryFromDir != null)
                    {
                        saves.Add(entryFromDir);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return [.. saves.OrderByDescending(entry => entry.LastAccessedTime)];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting saves");
        }
        return [];
    }

    /// <summary>
    ///     Parses the save name from the given command line arguments or defaults to the standard save name.
    /// </summary>
    // TODO : Remove this method once server hosting/loading happens as a service (see '.NET Generic Host' on msdn)
    public static string GetSaveName(string[] args, string defaultValue = null)
    {
        string result = args.GetCommandArgs("--save").FirstOrDefault() ?? args.GetCommandArgs("--name").FirstOrDefault();
        return IsValidSaveName(result) ? result : defaultValue;
    }

    private static bool IsValidSaveName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        if (name.StartsWith("--"))
        {
            return false;
        }
        if (name.EndsWith("."))
        {
            return false;
        }
        if (name.IndexOfAny(Path.GetInvalidFileNameChars().ToArray()) > -1)
        {
            return false;
        }
        return true;
    }
}

internal class ServerListing
{
    public string SaveDir { get; set; }
    public Version SaveVersion { get; set; }
    public DateTime LastAccessedTime { get; set; }

    internal static ServerListing? Validate(string saveDir)
    {
        ServerListing serverListing = new();
        if (!File.Exists(Path.Combine(saveDir, "server.cfg")))
        {
            return null;
        }

        SubnauticaServerConfig config = SubnauticaServerConfig.Load(saveDir);
        string fileEnding = config.SerializerMode switch
        {
            ServerSerializerMode.JSON => ServerJsonSerializer.FILE_ENDING,
            ServerSerializerMode.PROTOBUF => ServerProtoBufSerializer.FILE_ENDING,
            _ => throw new NotImplementedException()
        };

        string saveFileVersion = Path.Combine(saveDir, $"Version{fileEnding}");
        if (!File.Exists(saveFileVersion))
        {
            return null;
        }

        Version version;
        using (FileStream stream = new(saveFileVersion, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            version = config.SerializerMode switch
            {
                ServerSerializerMode.JSON => new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version,
                ServerSerializerMode.PROTOBUF => new ServerProtoBufSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version,
                _ => throw new NotImplementedException()
            };
        }

        serverListing.SaveDir = saveDir;
        serverListing.SaveVersion = version;
        serverListing.LastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(saveDir, $"PlayerData{fileEnding}"))
                                                                   ?
                                                                   // This file is affected by server saving
                                                                   Path.Combine(saveDir, $"PlayerData{fileEnding}")
                                                                   :
                                                                   // If the above file doesn't exist (server was never ran), use the Version file instead
                                                                   Path.Combine(saveDir, $"Version{fileEnding}"));

        return serverListing;
    }
}

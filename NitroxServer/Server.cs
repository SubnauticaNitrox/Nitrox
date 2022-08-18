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
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;
using Timer = System.Timers.Timer;

namespace NitroxServer
{
    public class Server
    {
        private readonly Communication.NitroxServer server;
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;
        private readonly Timer saveTimer;
        private readonly World world;
        private readonly EntityManager entityManager;
        private CancellationTokenSource serverCancelSource;

        public static Server Instance { get; private set; }

        public bool IsRunning => serverCancelSource?.IsCancellationRequested == false;
        public bool IsSaving { get; private set; }

        public int Port => serverConfig?.ServerPort ?? -1;

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NitroxServer server, EntityManager entityManager)
        {
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.server = server;
            this.world = world;
            this.entityManager = entityManager;

            Instance = this;

            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate
            {
                Save();
            };
        }

        public string GetSaveSummary(Perms viewerPerms = Perms.CONSOLE)
        {
            // TODO: Extend summary with more useful save file data
            // Note for later additions: order these lines by their length
            StringBuilder builder = new("\n");
            if (viewerPerms is Perms.CONSOLE)
            {
                builder.AppendLine($" - Save location: {Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName)}");
            }
            builder.AppendLine($" - Aurora's state: {world.EventTriggerer.GetAuroraStateSummary()}");
            builder.AppendLine($" - Current time: day {world.EventTriggerer.Day} ({Math.Floor(world.EventTriggerer.ElapsedSeconds)}s)");
            builder.AppendLine($" - Scheduled goals stored: {world.GameData.StoryGoals.ScheduledGoals.Count}");
            builder.AppendLine($" - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}");
            builder.AppendLine($" - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}");
            builder.AppendLine($" - World gamemode: {serverConfig.GameMode}");
            builder.AppendLine($" - Story goals unlocked: {world.GameData.StoryGoals.GoalUnlocks.Count}");
            builder.AppendLine($" - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}");
            builder.AppendLine($" - Storage slot items: {world.InventoryManager.GetAllStorageSlotItems().Count}");
            builder.AppendLine($" - Inventory items: {world.InventoryManager.GetAllInventoryItems().Count}");
            builder.AppendLine($" - Progress tech: {world.GameData.PDAState.CachedProgress.Count}");
            builder.AppendLine($" - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}");
            builder.AppendLine($" - Vehicles: {world.VehicleManager.GetVehicles().Count()}");
                
            return builder.ToString();
        }

        public static ServerConfig ServerStartHandler()
        {
            string saveDir = null;
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith(WorldManager.SavesFolderDir, StringComparison.OrdinalIgnoreCase) && Directory.Exists(arg))
                {
                    saveDir = arg;
                    break;
                }
            }
            if (saveDir == null)
            {
                // Check if there are any save files
                IEnumerable<WorldManager.Listing> WorldList = WorldManager.GetSaves();
                if (WorldList.Any())
                {
                    // Get last save file used
                    string lastSaveAccessed = WorldList.ElementAtOrDefault(0).WorldSaveDir;
                    if (WorldList.Count() > 1)
                    {
                        for (int i = 1; i < WorldList.Count(); i++)
                        {
                            if (File.GetLastWriteTime(Path.Combine(WorldList.ElementAtOrDefault(i).WorldSaveDir, "WorldData.json")) > File.GetLastWriteTime(lastSaveAccessed))
                            {
                                lastSaveAccessed = WorldList.ElementAtOrDefault(i).WorldSaveDir;
                            }
                        }
                    }
                    saveDir = lastSaveAccessed;
                }
                else
                {
                    // Create new save file
                    saveDir = Path.Combine(WorldManager.SavesFolderDir, "My World");
                    Directory.CreateDirectory(saveDir);
                    ServerConfig serverConfig = ServerConfig.Load(saveDir);
                    Log.Debug($"No save file was found, creating a new one...");
                }

            }

            return ServerConfig.Load(saveDir);
        }

        public void Save()
        {
            if (IsSaving)
            {
                return;
            }

            IsSaving = true;

            bool savedSuccessfully = worldPersistence.Save(world, Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName));
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

        public bool Start(CancellationTokenSource cancellationToken)
        {
            serverCancelSource = cancellationToken;
            if (!server.Start())
            {
                return false;
            }

            try
            {
                if (serverConfig.CreateFullEntityCache)
                {
                    Log.Info("Starting to load all batches up front.");
                    Log.Info("This can take up to several minutes and you can't join until it's completed.");
                    Log.Info($"{entityManager.GetAllEntities().Count} entities already cached");
                    if (entityManager.GetAllEntities().Count < 504732)
                    {
                        entityManager.LoadAllUnspawnedEntities(serverCancelSource.Token);

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
            
            LogHowToConnectAsync().ConfigureAwait(false);
            Log.Info($"Server is listening on port {Port} UDP");
            Log.Info($"Using {serverConfig.SerializerMode} as save file serializer");
            Log.InfoSensitive("Server Password: {password}", string.IsNullOrEmpty(serverConfig.ServerPassword) ? "None. Public Server." : serverConfig.ServerPassword);
            Log.InfoSensitive("Admin Password: {password}", serverConfig.AdminPassword);
            Log.Info($"Autosave: {(serverConfig.DisableAutoSave ? "DISABLED" : $"ENABLED ({serverConfig.SaveInterval / 60000} min)")}");
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

            serverCancelSource.Cancel();
            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicSaving();

            if (shouldSave)
            {
                Save();
            }

            server.Stop();
            Log.Info("Nitrox Server Stopped");
        }

        private async Task LogHowToConnectAsync()
        {
            Task<IPAddress> localIp = Task.Factory.StartNew(NetHelper.GetLanIp);
            Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
            Task<IPAddress> hamachiIp = Task.Factory.StartNew(NetHelper.GetHamachiIp);

            List<string> options = new();
            options.Add("127.0.0.1 - You (Local)");
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
            world.EventTriggerer.PauseWorld();
            Log.Info("Server has paused, waiting for players to connect");
        }

        public void ResumeServer()
        {
            if (!serverConfig.DisableAutoSave)
            {
                EnablePeriodicSaving();
            }
            world.EventTriggerer.StartWorld();
            Log.Info("Server has resumed");
        }
    }
}

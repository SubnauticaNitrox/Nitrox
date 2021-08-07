using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer
{
    public class Server
    {
        private readonly Communication.NitroxServer server;
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;
        private readonly Timer saveTimer;
        private readonly World world;

        public static Server Instance { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsSaving { get; private set; }

        public int Port => serverConfig?.ServerPort ?? -1;

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NitroxServer server)
        {
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.server = server;
            this.world = world;

            Instance = this;

            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate
            {
                Save();
            };
        }

        public string SaveSummary
        {
            get
            {
                // TODO: Extend summary with more useful save file data
                StringBuilder builder = new("\n");
                builder.AppendLine($" - Save location: {Path.GetFullPath(serverConfig.SaveName)}");
                builder.AppendLine($" - World GameMode: {serverConfig.GameMode}");
                builder.AppendLine($" - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}");
                builder.AppendLine($" - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}");
                builder.AppendLine($" - Story goals unlocked: {world.GameData.StoryGoals.GoalUnlocks.Count}");
                builder.AppendLine($" - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}");
                builder.AppendLine($" - Storage slot items: {world.InventoryManager.GetAllStorageSlotItems().Count}");
                builder.AppendLine($" - Inventory items: {world.InventoryManager.GetAllInventoryItems().Count}");
                builder.AppendLine($" - Partially unlocked: {world.GameData.PDAState.PartiallyUnlockedByTechType.Count}");
                builder.AppendLine($" - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}");
                builder.AppendLine($" - Vehicles: {world.VehicleManager.GetVehicles().Count()}");

                return builder.ToString();
            }
        }

        public void Save()
        {
            if (IsSaving)
            {
                return;
            }

            IsSaving = true;

            bool savedSuccessfully = worldPersistence.Save(world, serverConfig.SaveName);
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

        public bool Start()
        {
            if (!server.Start())
            {
                return false;
            }

            Log.Info($"Server is listening on port {Port} UDP");
            Log.Info($"Using {serverConfig.SerializerMode} as save file serializer");
            Log.InfoSensitive("Server Password: {password}", string.IsNullOrEmpty(serverConfig.ServerPassword) ? "None. Public Server." : serverConfig.ServerPassword);
            Log.InfoSensitive("Admin Password: {password}", serverConfig.AdminPassword);
            Log.Info($"Autosave: {(serverConfig.DisableAutoSave ? "DISABLED" : $"ENABLED ({serverConfig.SaveInterval / 60000} min)")}");
            Log.Info($"Loaded save\n{SaveSummary}");

            PauseServer();

            IsRunning = true;
#if RELEASE
            IpLogger.PrintServerIps();
#endif
            return true;
        }

        public void Stop(bool shouldSave = true)
        {
            if (!IsRunning)
            {
                return;
            }

            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicSaving();

            if (shouldSave)
            {
                Save();
            }

            server.Stop();
            Log.Info("Nitrox Server Stopped");
            IsRunning = false;
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
            world.EventTriggerer.PauseWorldTime();
            world.EventTriggerer.PauseEventTimers();
            Log.Info("Server has paused, waiting for players to connect");
        }

        public void ResumeServer()
        {
            if (!serverConfig.DisableAutoSave)
            {
                EnablePeriodicSaving();
            }
            world.EventTriggerer.StartWorldTime();
            world.EventTriggerer.StartEventTimers();
            Log.Info("Server has resumed");
        }
    }
}

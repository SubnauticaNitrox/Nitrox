using System;
using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;
using System.Configuration;
using System.Text;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private readonly Communication.NetworkingLayer.NitroxServer server;
        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        public bool IsRunning { get; private set; }
        private bool isSaving;
        public static Server Instance { get; private set; }

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                Log.Warn("Nitrox Server Cant Read Config File.");
            }
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;
            this.server = server;
            
            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate { Save(); };
        }

        public string SaveSummary
        {
            get
            {
                // TODO: Extend summary with more useful save file data
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($" - Game mode: {world.GameMode}");
                builder.AppendLine($" - Inventory items: {world.InventoryManager.GetAllInventoryItems().Count}");
                builder.AppendLine($" - Storage slot items: {world.InventoryManager.GetAllStorageSlotItems().Count}");
                builder.AppendLine($" - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}");
                builder.AppendLine($" - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}");
                builder.AppendLine($" - Story goals unlocked: {world.GameData.StoryGoals.GoalUnlocks.Count}");
                builder.AppendLine($" - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}");
                builder.AppendLine($" - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}");
                return builder.ToString();
            }
        }

        public void Save()
        {
            if (isSaving)
            {
                return;
            }
            isSaving = true;
            worldPersistence.Save(world);
            isSaving = false;
        }

        public bool Start()
        {
            if (!server.Start())
            {
                return false;
            }
            Log.Info("Nitrox Server Started");
            IsRunning = true;
            EnablePeriodicSaving();
            
#if RELEASE
            // Help new players on which IP they should give to their friends
            IpLogger.PrintServerIps();
#endif
            
            return true;
        }

        public void Stop()
        {
            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicSaving();
            Save();
            server.Stop();
            Log.Info("Nitrox Server Stopped");
            IsRunning = false;
        }

        public void EnablePeriodicSaving()
        {
            saveTimer.Start();
        }

        public void DisablePeriodicSaving()
        {
            saveTimer.Stop();
        }
    }
}

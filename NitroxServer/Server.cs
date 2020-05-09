using System.Timers;
using NitroxModel.Logger;
using NitroxModel.Server;
using NitroxServer.Serialization.World;
using System.Configuration;
using System.Text;
using System.Linq;

namespace NitroxServer
{
    public class Server
    {
        private readonly Communication.NetworkingLayer.NitroxServer server;
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;
        private readonly Timer saveTimer;
        private readonly World world;

        public static Server Instance { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsSaving { get; private set; }

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                Log.Warn("Nitrox Server Cant Read Config File.");
            }

            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.server = server;
            this.world = world;

            Instance = this;

            // TODO: Save once after last player leaves then stop saving.
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
                StringBuilder builder = new StringBuilder("\n");
                builder.AppendLine($" - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}");
                builder.AppendLine($" - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}");
                builder.AppendLine($" - Story goals unlocked: {world.GameData.StoryGoals.GoalUnlocks.Count}");
                builder.AppendLine($" - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}");
                builder.AppendLine($" - Storage slot items: {world.InventoryManager.GetAllStorageSlotItems().Count}");
                builder.AppendLine($" - Inventory items: {world.InventoryManager.GetAllInventoryItems().Count}");
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
            worldPersistence.Save(world);
            IsSaving = false;
        }

        public bool Start()
        {
            if (!server.Start())
            {
                return false;
            }

            Log.Info("Nitrox Server Started");

            if (!serverConfig.DisableAutoSave)
            {
                EnablePeriodicSaving();
            }

            IsRunning = true;
#if RELEASE
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

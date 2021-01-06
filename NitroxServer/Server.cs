using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Serialization.World;
using System.IO;
using System.Text;
using System.Linq;
using NitroxServer.Serialization;
using NitroxModel.Serialization;

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

        public int Port => serverConfig?.ServerPort ?? -1;

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.server = server;
            this.world = world;

            Instance = this;

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
                builder.AppendLine($" - Save location: {Path.GetFullPath(serverConfig.SaveName)}");
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
            NitroxConfig.Serialize(serverConfig); // This is overwriting the config file => server has to be closed before making changes to it
            worldPersistence.Save(world, serverConfig.SaveName);
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
            Log.Info($"World GameMode: {serverConfig.GameMode}");
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
            Log.Info("Server has paused");
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

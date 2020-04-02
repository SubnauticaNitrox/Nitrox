using System.Timers;
using NitroxModel.Logger;
using NitroxModel.Server;
using System.Configuration;
using NitroxServer.Serialization.World;
using System;
using System.Linq;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private Communication.NetworkingLayer.NitroxServer server;
        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;

        public bool IsRunning { get; private set; }
        public bool isSaving { get; private set; }

        public static Server Instance { get; private set; }

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                Log.Warn("Nitrox Server Cant Read Config File.");
            }

            if (Instance != null)
            {
                throw new Exception("An instance of Server has already been defined");
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
            saveTimer.Elapsed += delegate
            { Save(); };
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

        public void Start()
        {
            IsRunning = true;
            IpLogger.PrintServerIps();
            server.Start();
            Log.Info("Nitrox Server Started");
            EnablePeriodicSaving();
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

        private void EnablePeriodicSaving()
        {
            saveTimer.Start();
        }

        private void DisablePeriodicSaving()
        {
            saveTimer.Stop();
        }
    }
}

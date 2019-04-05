using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private Communication.NetworkingLayer.NitroxServer server;
        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        public bool IsRunning { get; private set; }
        private bool isSaving;
        public static Server Instance { get; private set; }
        public System.Windows.Forms.Control DelegateControl { get; set; }

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;
            this.server = server;
            
            
            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate { Save(); };
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
            server.DelegateControl = DelegateControl;
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

using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;
using System.Configuration;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private Communication.NetworkingLayer.NitroxServer server;
        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        public bool IsRunning { get; private set; }
        private bool IsSaving;
        public static Server Instance { get; private set; }

        public Server(WorldPersistence worldPersistence, World world, ServerConfig serverConfig, Communication.NetworkingLayer.NitroxServer server)
        {
            if (ConfigurationManager.AppSettings.Count == 0)
            {
                Log2.Instance.Log(NLogType.Warn, "Nitrox Server Cant Read Config File.");
            }
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;
            this.server = server;
            
            // TODO: Save once after last player leaves then stop saving.
            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate { Save(); };
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

        public void Start()
        {
            IsRunning = true;
            IpLogger.PrintServerIps();
            server.Start();
            Log2.Instance.Log(NLogType.Info, "Nitrox Server Started");
            EnablePeriodicSaving();
        }

        public void Stop()
        {
            Log2.Instance.Log(NLogType.Info, "Nitrox Server Stopping...");
            DisablePeriodicSaving();
            Save();
            server.Stop();
            Log2.Instance.Log(NLogType.Info, "Nitrox Server Stopped");
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

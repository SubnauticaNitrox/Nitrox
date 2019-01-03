using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Serialization.World;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private readonly UdpServer udpServer;

        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        public bool IsRunning { get; private set; }
        public static Server Instance { get; private set; }

        public Server(WorldPersistence worldPersistence, World world, UdpServer udpServer)
        {
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;

            this.udpServer = udpServer;

            //Maybe add settings for the interval?
            saveTimer = new Timer();
            saveTimer.Interval = 60000;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate { Save(); };
        }

        public void Save()
        {
            worldPersistence.Save(world);
        }

        public void Start()
        {
            IsRunning = true;
            IpLogger.PrintServerIps();
            udpServer.Start();
            Log.Info("Nitrox Server Started");
            EnablePeriodicSaving();
        }

        public void Stop()
        {
            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicSaving();
            Save();
            udpServer.Stop();
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

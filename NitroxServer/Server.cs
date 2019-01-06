using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;
using System;

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

        public Server(WorldPersistence worldPersistence, World world, UdpServer udpServer, ServerConfig serverConfig)
        {
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;

            this.udpServer = udpServer;

            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
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
            Log.Info("Nitrox Server Started!");
            EnablePeriodicSaving();
        }

        public void Stop()
        {
            Log.Info("Nitrox Server Stopping:");
            Log.Warn("Disabling periodic saving...");
            DisablePeriodicSaving();
            Log.Info("Periodic saving disabled.");
            Log.Info("This is old \"stop\" method, so world will be saved.");
            Log.Info("Saving world...");
            Save();
            Log.Info("World saved.");
            Log.Info("Stopping UDP...");
            udpServer.Stop();
            Log.Info("UDP stopped.");
            Log.Info("Nitrox Server Stopped.");
            IsRunning = false;
        }

        public void StopNosave()
        {
            Log.Info("Nitrox Server Stopping:");
            Log.Warn("Disabling periodic saving...");
            DisablePeriodicSaving();
            Log.Info("Periodic saving disabled.");
            Log.Info("This is new \"stop\" method, so world will not be saved. Server's program is going to jump directly to UDP.");
            Console.WriteLine("By the way, don't worry. If you used \"save s\", \"save o\" (and clicked \"Y\"), etc. your world is arleady saved. \"This is new \"stop\" method, so world will not be saved. Server's program is going to jump directly to UDP.\" just means, that the world is not saved here, inside \"stop\" method. So don't be affraid.");
            Log.Info("Stopping UDP...");
            udpServer.Stop();
            Log.Info("UDP stopped.");
            Log.Info("Nitrox Server Stopped.");
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

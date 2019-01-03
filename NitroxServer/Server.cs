using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.Serialization.World;
using System.Timers;
using NitroxServer.ConfigParser;
using NitroxServer.ConsoleCommands.Processor;
using NitroxModel.Core;


namespace NitroxServer
{
    public class Server
    {
        public bool IsRunning { get; private set; }

        private readonly World world;
        private readonly UdpServer udpServer;
        private readonly WorldPersistence worldPersistence;
        private readonly PacketHandler packetHandler;
        private readonly Timer saveTimer;
        private ServerConfig serverConfiguration;
        public static Server Instance;

        public Server(WorldPersistence worldPersistence, World world, ServerConfig config, UdpServer udpServer, PacketHandler packetHandler)
        {
            serverConfiguration = config;
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;

            this.udpServer = udpServer;
            this.packetHandler = packetHandler;

            //Maybe add settings for the interval?
            saveTimer = new Timer();
            saveTimer.Interval = 60000;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate
            {
                Save();
            };
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

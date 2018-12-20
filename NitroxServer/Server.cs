using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.Serialization.World;
using System.Timers;
using NitroxServer.ConfigParser;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;


namespace NitroxServer
{
    public class Server
    {
        private readonly World world;
        private readonly UdpServer udpServer;
        private readonly WorldPersistence worldPersistence;
        private readonly PacketHandler packetHandler;
        private readonly Timer saveTimer;
        private ServerConfig serverConfiguration;
        public static Server Instance;

        public void Save()
        {
            worldPersistence.Save(world);
        }

        public Server(ServerConfig config)
        {
            serverConfiguration = config;
            Instance = this;
            worldPersistence = new WorldPersistence();
            world = worldPersistence.Load();
            packetHandler = new PacketHandler(world);
            udpServer = new UdpServer(packetHandler, world.PlayerManager, world.EntitySimulation, serverConfiguration);

            //Maybe add settings for the interval?
            saveTimer = new Timer();
            saveTimer.Interval = 60000;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate
            {
                Save();
            };
        }

        public void Start()
        {
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

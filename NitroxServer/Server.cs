using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.Serialization.World;
using System.Timers;

namespace NitroxServer
{
    public class Server
    {
        private readonly World world;
        private readonly UdpServer udpServer;
        private readonly WorldPersistence worldPersistence;
        private readonly PacketHandler packetHandler;

        public static Server Instance;

        public void Save()
        {
            worldPersistence.Save(world);
        }

        public Server()
        {
            Instance = this;
            worldPersistence = new WorldPersistence();
            world = worldPersistence.Load();
            packetHandler = new PacketHandler(world);
            udpServer = new UdpServer(packetHandler, world.PlayerManager, world.EntitySimulation);            
        }

        public void Start()
        {
            udpServer.Start();
            Log.Info("Nitrox Server Started");
            EnablePeriodicSaving();
        }
        
        private void EnablePeriodicSaving()
        {
            Timer timer = new Timer();
            timer.Elapsed += delegate
            {
                worldPersistence.Save(world);
            };
            timer.Interval = 60000;
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Start();
        }
    }
}

using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using System.Configuration;

namespace NitroxServer
{
    public class Server
    {
        private readonly TcpServer tcpServer;

        public Server()
        {
            TimeKeeper timeKeeper = new TimeKeeper();
            SimulationOwnership simulationOwnership = new SimulationOwnership();
            PlayerManager playerManager = new PlayerManager();
            PacketHandler packetHandler = new PacketHandler(playerManager, timeKeeper, simulationOwnership);
            EventTriggerer eventTriggerer = new EventTriggerer(playerManager);


            tcpServer = new TcpServer(packetHandler, playerManager);
        }

        public void Start()
        {
            int port = int.Parse(ConfigurationManager.AppSettings.Get("port"));
            int pEnd = 4000;
            tcpServer.Start(port, pEnd);
            Log.Info("Starting Nitrox server on port " + port);
            Log.Info("Nitrox Server Started");
            
        }
    }
}

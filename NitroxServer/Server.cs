using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;

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

            tcpServer = new TcpServer(packetHandler, playerManager);
        }

        public void Start()
        {
            Log.Info("Starting Nitrox Server");
            tcpServer.Start();
        }
    }
}

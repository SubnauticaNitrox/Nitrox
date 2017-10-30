using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer
{
    public class Server
    {
        private readonly TcpServer tcpServer = new TcpServer();
        private readonly TimeKeeper timeKeeper = new TimeKeeper();
        private readonly SimulationOwnership simulationOwnership = new SimulationOwnership();
        private readonly PacketHandler packetHandler;

        public Server()
        {
            packetHandler = new PacketHandler(tcpServer, timeKeeper, simulationOwnership);
        }

        public void Start()
        {
            Log.Info("Starting Nitrox Server");
            tcpServer.Start(packetHandler);
        }
    }
}

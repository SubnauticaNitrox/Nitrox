using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer
{
    public class Server
    {
        public static Logic Logic { get; private set; }

        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private SimulationOwnership simulationOwnership;
        private PacketHandler packetHandler;

        public Server()
        {
            this.timeKeeper = new TimeKeeper();
            this.tcpServer = new TcpServer();
            this.simulationOwnership = new SimulationOwnership();
            this.packetHandler = new PacketHandler(tcpServer, timeKeeper, simulationOwnership);

            Logic = new Logic(tcpServer);
        }

        public void Start()
        {
            tcpServer.Start(packetHandler);
        }
    }
}

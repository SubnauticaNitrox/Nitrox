using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer
{
    public class Server
    {
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
        }        

        public void Start()
        {
            Log.Info("Starting Nitrox Server");
            tcpServer.Start(packetHandler);
        }
    }    
}

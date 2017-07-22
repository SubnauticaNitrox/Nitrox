using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer
{
    public class Server
    {
        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private PacketHandler packetHandler;

        public Server()
        {
            this.timeKeeper = new TimeKeeper();
            this.tcpServer = new TcpServer();
            this.packetHandler = new PacketHandler(tcpServer, timeKeeper);
        }

        public void Start()
        {
            tcpServer.Start(packetHandler);
        }
    }
}

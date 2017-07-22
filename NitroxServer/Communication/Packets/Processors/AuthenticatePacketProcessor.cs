using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System;

namespace NitroxServer.Communication.Packets.Processors
{
    public class AuthenticatePacketProcessor : UnauthenticatedPacketProcessor<Authenticate>
    {
        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;

        public AuthenticatePacketProcessor(TcpServer tcpServer, TimeKeeper timeKeeper)
        {
            this.tcpServer = tcpServer;
            this.timeKeeper = timeKeeper;
        }

        public override void Process(Authenticate packet, PlayerConnection connection)
        {
            Console.WriteLine("sending time: " + timeKeeper.GetCurrentTime());

            Player player = new Player(packet.PlayerId);

            tcpServer.PlayerAuthenticated(player, connection);
            tcpServer.SendPacketToPlayer(new TimeChange(timeKeeper.GetCurrentTime()), player);
        }
    }
}

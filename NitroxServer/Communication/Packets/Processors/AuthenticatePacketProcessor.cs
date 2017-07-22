using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System;

namespace NitroxServer.Communication.Packets.Processors
{
    public class AuthenticatePacketProcessor : GenericServerPacketProcessor<Authenticate>
    {
        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;

        public AuthenticatePacketProcessor(TcpServer tcpServer, TimeKeeper timeKeeper)
        {
            this.tcpServer = tcpServer;
            this.timeKeeper = timeKeeper;
        }

        public override void Process(Authenticate packet, Player player)
        {
            Console.WriteLine("sending time: " + timeKeeper.GetCurrentTime());
            tcpServer.SendPacketToAllPlayers(new TimeChange(timeKeeper.GetCurrentTime()));
        }
    }
}

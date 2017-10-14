using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class AuthenticatePacketProcessor : UnauthenticatedPacketProcessor<Authenticate>
    {
        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private EscapePodManager escapePodManager;

        public AuthenticatePacketProcessor(TcpServer tcpServer, TimeKeeper timeKeeper, EscapePodManager escapePodManager)
        {
            this.tcpServer = tcpServer;
            this.timeKeeper = timeKeeper;
            this.escapePodManager = escapePodManager;
        }

        public override void Process(Authenticate packet, PlayerConnection connection)
        {
            Player player = new Player(packet.PlayerId);

            tcpServer.PlayerAuthenticated(player, connection);
            tcpServer.SendPacketToPlayer(new TimeChange(timeKeeper.GetCurrentTime()), player);

            escapePodManager.AssignPlayerToEscapePod(packet.PlayerId);

            BroadcastEscapePods broadcastEscapePods = new BroadcastEscapePods(escapePodManager.GetEscapePods());
            tcpServer.SendPacketToAllPlayers(broadcastEscapePods);
        }
    }
}

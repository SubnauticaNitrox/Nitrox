using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class AuthenticatePacketProcessor : UnauthenticatedPacketProcessor<Authenticate>
    {
        private readonly TimeKeeper timeKeeper;
        private readonly EscapePodManager escapePodManager;
        private readonly PlayerManager playerManager;

        public AuthenticatePacketProcessor(TimeKeeper timeKeeper, EscapePodManager escapePodManager, PlayerManager playerManager)
        {
            this.timeKeeper = timeKeeper;
            this.escapePodManager = escapePodManager;
            this.playerManager = playerManager;
        }

        public override void Process(Authenticate packet, Connection connection)
        {
            Player player = playerManager.PlayerAuthenticated(connection, packet.PlayerId);            
            player.SendPacket(new TimeChange(timeKeeper.GetCurrentTime()));

            escapePodManager.AssignPlayerToEscapePod(player.Id);

            BroadcastEscapePods broadcastEscapePods = new BroadcastEscapePods(escapePodManager.GetEscapePods());
            playerManager.SendPacketToAllPlayers(broadcastEscapePods);
        }
    }
}

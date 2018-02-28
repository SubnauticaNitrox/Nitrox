using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly TimeKeeper timeKeeper;
        private readonly EscapePodManager escapePodManager;
        private readonly PlayerManager playerManager;

        public PlayerJoiningMultiplayerSessionProcessor(TimeKeeper timeKeeper, EscapePodManager escapePodManager,
            PlayerManager playerManager)
        {
            this.timeKeeper = timeKeeper;
            this.escapePodManager = escapePodManager;
            this.playerManager = playerManager;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, Connection connection)
        {
            Player player = playerManager.CreatePlayer(connection, packet.ReservationKey);
            player.SendPacket(new TimeChange(timeKeeper.GetCurrentTime()));

            escapePodManager.AssignPlayerToEscapePod(player.Id);

            BroadcastEscapePods broadcastEscapePods = new BroadcastEscapePods(escapePodManager.GetEscapePods());
            playerManager.SendPacketToAllPlayers(broadcastEscapePods);

            PlayerJoinedMultiplayerSession playerJoinedPacket = new PlayerJoinedMultiplayerSession(player.Id, player.Name, player.PlayerSettings);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            foreach (Player otherPlayer in playerManager.GetPlayers())
            {
                if (!player.Equals(otherPlayer))
                {
                    playerJoinedPacket = new PlayerJoinedMultiplayerSession(otherPlayer.Id, otherPlayer.Name, otherPlayer.PlayerSettings);
                    player.SendPacket(playerJoinedPacket);
                }
            }
        }
    }
}

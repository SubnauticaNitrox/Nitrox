using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerJoiningMultiplayerSessionProcessor : UnauthenticatedPacketProcessor<PlayerJoiningMultiplayerSession>
    {
        private readonly TimeKeeper timeKeeper;
        private readonly EscapePodManager escapePodManager;
        private readonly PlayerManager playerManager;
        private readonly BaseData baseData;

        public PlayerJoiningMultiplayerSessionProcessor(TimeKeeper timeKeeper, EscapePodManager escapePodManager,
            PlayerManager playerManager, BaseData baseData)
        {
            this.timeKeeper = timeKeeper;
            this.escapePodManager = escapePodManager;
            this.playerManager = playerManager;
            this.baseData = baseData;
        }

        public override void Process(PlayerJoiningMultiplayerSession packet, Connection connection)
        {
            Player player = playerManager.CreatePlayer(connection, packet.ReservationKey);
            player.SendPacket(new TimeChange(timeKeeper.GetCurrentTime()));

            escapePodManager.AssignPlayerToEscapePod(player.Id);

            BroadcastEscapePods broadcastEscapePods = new BroadcastEscapePods(escapePodManager.GetEscapePods());
            playerManager.SendPacketToAllPlayers(broadcastEscapePods);

            PlayerJoinedMultiplayerSession playerJoinedPacket = new PlayerJoinedMultiplayerSession(player.PlayerContext);
            playerManager.SendPacketToOtherPlayers(playerJoinedPacket, player);

            InitialPlayerSync initialPlayerSync = new InitialPlayerSync(baseData.GetBasePiecesForNewlyConnectedPlayer());
            player.SendPacket(initialPlayerSync);

            foreach (Player otherPlayer in playerManager.GetPlayers())
            {
                if (!player.Equals(otherPlayer))
                {
                    playerJoinedPacket = new PlayerJoinedMultiplayerSession(otherPlayer.PlayerContext);
                    player.SendPacket(playerJoinedPacket);
                }
            }
        }
    }
}

using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerSyncFinishedProcessor : UnauthenticatedPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager;

        public PlayerSyncFinishedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerSyncFinished packet, NitroxConnection connection)
        {
            playerManager.PlayerCurrentlyJoining = false;

            var keyValuePair = playerManager.JoinQueue.Dequeue();
            NitroxConnection requestConnection = keyValuePair.Key;
            MultiplayerSessionReservationRequest request = keyValuePair.Value;

            playerManager.ReservePlayerContext(requestConnection, request.PlayerSettings, request.AuthenticationContext, request.CorrelationId);
        }
    }
}

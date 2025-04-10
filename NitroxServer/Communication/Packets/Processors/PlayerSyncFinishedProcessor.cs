using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerSyncFinishedProcessor : AuthenticatedPacketProcessor<PlayerSyncFinished>
    {
        private readonly PlayerManager playerManager;
        private readonly JoiningManager joiningManager;

        public PlayerSyncFinishedProcessor(PlayerManager playerManager, JoiningManager joiningManager)
        {
            this.playerManager = playerManager;
            this.joiningManager = joiningManager;
        }

        public override void Process(PlayerSyncFinished packet, Player player)
        {
            // If this is the first player connecting we need to restart time at this exact moment
            if (playerManager.GetConnectedPlayers().Count == 1)
            {
                Server.Instance.ResumeServer();
            }

            joiningManager.SyncFinishedCallback?.Invoke();
        }
    }
}

using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PingRenamedPacketProcessor : AuthenticatedPacketProcessor<PingRenamed>
    {
        private readonly BaseManager baseManager;
        private readonly PlayerManager playerManager;

        public PingRenamedPacketProcessor(BaseManager baseManager, PlayerManager playerManager)
        {
            this.baseManager = baseManager;
            this.playerManager = playerManager;
        }

        public override void Process(PingRenamed packet, Player player)
        {
            // TODO: Fix persist

            Log.Info($"Received ping rename: {packet} by player: {player}");
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

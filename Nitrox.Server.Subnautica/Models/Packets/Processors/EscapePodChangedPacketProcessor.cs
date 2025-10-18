using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class EscapePodChangedPacketProcessor : AuthenticatedPacketProcessor<EscapePodChanged>
    {
        private readonly PlayerManager playerManager;

        public EscapePodChangedPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(EscapePodChanged packet, Player player)
        {
            Log.Debug(packet);
            player.SubRootId = packet.EscapePodId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

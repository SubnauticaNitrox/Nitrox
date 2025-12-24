using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class PDAEncyclopediaEntryAddProcessor : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PdaManager pdaManager;

        public PDAEncyclopediaEntryAddProcessor(PlayerManager playerManager, PdaManager pdaManager)
        {
            this.playerManager = playerManager;
            this.pdaManager = pdaManager;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet, Player player)
        {
            pdaManager.AddEncyclopediaEntry(packet.Key);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

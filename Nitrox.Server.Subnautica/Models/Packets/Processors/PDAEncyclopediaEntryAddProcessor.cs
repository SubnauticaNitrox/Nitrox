using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class PDAEncyclopediaEntryAddProcessor : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PdaStateData pdaStateData;

        public PDAEncyclopediaEntryAddProcessor(PlayerManager playerManager, PdaStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PDAEncyclopediaEntryAdd packet, Player player)
        {
            pdaStateData.AddEncyclopediaEntry(packet.Key);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

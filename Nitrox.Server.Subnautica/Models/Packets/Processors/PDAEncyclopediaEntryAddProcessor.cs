using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class PDAEncyclopediaEntryAddProcessor : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public PDAEncyclopediaEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
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

using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class KnownTechEntryAddProcessor : AuthenticatedPacketProcessor<KnownTechEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PdaManager pdaManager;

        public KnownTechEntryAddProcessor(PlayerManager playerManager, PdaManager pdaManager)
        {
            this.playerManager = playerManager;
            this.pdaManager = pdaManager;
        }

        public override void Process(KnownTechEntryAdd packet, Player player)
        {
            switch (packet.Category)
            {
                case KnownTechEntryAdd.EntryCategory.KNOWN:
                    pdaManager.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
                    break;
                case KnownTechEntryAdd.EntryCategory.ANALYZED:
                    pdaManager.AddAnalyzedTechType(packet.TechType);
                    break;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

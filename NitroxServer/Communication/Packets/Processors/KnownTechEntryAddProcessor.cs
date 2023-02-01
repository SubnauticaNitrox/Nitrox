using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class KnownTechEntryAddProcessor : AuthenticatedPacketProcessor<KnownTechEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;

        public KnownTechEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(KnownTechEntryAdd packet, Player player)
        {
            switch (packet.Category)
            {
                case KnownTechEntryAdd.EntryCategory.KNOWN:
                    pdaStateData.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
                    break;
                case KnownTechEntryAdd.EntryCategory.ANALYZED:
                    pdaStateData.AddAnalyzedTechType(packet.TechType);
                    break;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}

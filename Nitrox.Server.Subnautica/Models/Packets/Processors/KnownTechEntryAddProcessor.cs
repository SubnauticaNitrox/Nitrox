using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class KnownTechEntryAddProcessor(IPacketSender packetSender, PdaManager pdaManager) : AuthenticatedPacketProcessor<KnownTechEntryAdd>
{
    private readonly PdaManager pdaManager = pdaManager;

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

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}

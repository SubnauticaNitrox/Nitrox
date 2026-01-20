using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAEncyclopediaEntryAddProcessor(IPacketSender packetSender, PdaManager pdaManager) : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PdaManager pdaManager = pdaManager;

    public override void Process(PDAEncyclopediaEntryAdd packet, Player player)
    {
        pdaManager.AddEncyclopediaEntry(packet.Key);
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}

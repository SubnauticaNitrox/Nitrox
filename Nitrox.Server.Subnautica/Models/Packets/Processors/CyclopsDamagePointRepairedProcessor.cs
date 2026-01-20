using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CyclopsDamagePointRepairedProcessor(IPacketSender packetSender) : AuthenticatedPacketProcessor<CyclopsDamagePointRepaired>
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(CyclopsDamagePointRepaired packet, Player simulatingPlayer)
    {
        packetSender.SendPacketToOthersAsync(packet, simulatingPlayer.SessionId);
    }
}

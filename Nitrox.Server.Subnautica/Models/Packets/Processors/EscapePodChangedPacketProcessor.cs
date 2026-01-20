using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EscapePodChangedPacketProcessor(IPacketSender packetSender) : AuthenticatedPacketProcessor<EscapePodChanged>
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(EscapePodChanged packet, Player player)
    {
        player.SubRootId = packet.EscapePodId;
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}

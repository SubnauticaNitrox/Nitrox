using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CyclopsFireCreatedProcessor(IPacketSender packetSender) : AuthenticatedPacketProcessor<CyclopsFireCreated>
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(CyclopsFireCreated packet, Player simulatingPlayer)
    {
        packetSender.SendPacketToOthersAsync(packet, simulatingPlayer.SessionId);
    }
}

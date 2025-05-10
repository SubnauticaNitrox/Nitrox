using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonGrabExosuitProcessor(
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonGrabExosuit>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaDragonGrabExosuit packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.SeaDragonId, packet.TargetId);
}

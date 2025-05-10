using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CreaturePoopPerformed packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId);
}

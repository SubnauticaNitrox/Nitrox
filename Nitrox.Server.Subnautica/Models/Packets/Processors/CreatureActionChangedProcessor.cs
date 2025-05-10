using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreatureActionChangedProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreatureActionChanged>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CreatureActionChanged packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId);
}

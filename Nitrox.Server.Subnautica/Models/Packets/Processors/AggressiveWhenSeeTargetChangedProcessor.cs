using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AggressiveWhenSeeTargetChangedProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AggressiveWhenSeeTargetChanged>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, AggressiveWhenSeeTargetChanged packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId, packet.TargetId);
}

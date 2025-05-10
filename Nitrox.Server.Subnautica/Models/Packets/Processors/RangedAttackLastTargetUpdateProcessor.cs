using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RangedAttackLastTargetUpdateProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<RangedAttackLastTargetUpdate>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, RangedAttackLastTargetUpdate packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId, packet.TargetId);
}

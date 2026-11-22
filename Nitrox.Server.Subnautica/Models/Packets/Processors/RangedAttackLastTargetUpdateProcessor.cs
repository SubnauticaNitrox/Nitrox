using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RangedAttackLastTargetUpdateProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<RangedAttackLastTargetUpdate>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, RangedAttackLastTargetUpdate packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId, packet.TargetId]);
}

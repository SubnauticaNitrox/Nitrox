using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonAttackTarget>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaDragonAttackTarget packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.SeaDragonId, packet.TargetId]);
}

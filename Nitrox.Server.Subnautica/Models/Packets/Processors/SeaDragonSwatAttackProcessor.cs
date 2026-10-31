using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaDragonSwatAttack packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.SeaDragonId, packet.TargetId]);
}

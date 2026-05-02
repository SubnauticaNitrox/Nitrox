using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaDragonSwatAttack packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.SeaDragonId, packet.TargetId]);
}

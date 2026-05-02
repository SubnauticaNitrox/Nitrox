using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AttackCyclopsTargetChangedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<AttackCyclopsTargetChanged>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, AttackCyclopsTargetChanged packet)
    {
        await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId, packet.TargetId]);
    }
}

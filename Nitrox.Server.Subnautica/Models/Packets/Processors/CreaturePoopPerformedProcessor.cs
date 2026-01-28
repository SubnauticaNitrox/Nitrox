using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CreaturePoopPerformed packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId]);
}

using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(playerManager, entityRegistry)
{
    public override void Process(CreaturePoopPerformed packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}

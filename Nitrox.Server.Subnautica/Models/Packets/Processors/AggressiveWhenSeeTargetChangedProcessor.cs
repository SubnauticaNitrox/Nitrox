using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class AggressiveWhenSeeTargetChangedProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AggressiveWhenSeeTargetChanged>(playerManager, entityRegistry)
{
    public override void Process(AggressiveWhenSeeTargetChanged packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId, packet.TargetId]);
}

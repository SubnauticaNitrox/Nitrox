using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreatureActionChangedProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreatureActionChanged>(playerManager, entityRegistry)
{
    public override void Process(CreatureActionChanged packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}

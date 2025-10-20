using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonAttackTarget>(playerManager, entityRegistry)
{
    public override void Process(SeaDragonAttackTarget packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.SeaDragonId, packet.TargetId]);
}

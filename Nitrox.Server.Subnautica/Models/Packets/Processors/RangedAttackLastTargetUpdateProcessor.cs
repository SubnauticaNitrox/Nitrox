using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class RangedAttackLastTargetUpdateProcessor(
    IPacketSender packetSender,
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<RangedAttackLastTargetUpdate>(packetSender, playerManager, entityRegistry)
{
    public override void Process(RangedAttackLastTargetUpdate packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId, packet.TargetId]);
}

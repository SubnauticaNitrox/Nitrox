using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor(
    IPacketSender packetSender,
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonAttackTarget>(packetSender, playerManager, entityRegistry)
{
    public override void Process(SeaDragonAttackTarget packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.SeaDragonId, packet.TargetId]);
}

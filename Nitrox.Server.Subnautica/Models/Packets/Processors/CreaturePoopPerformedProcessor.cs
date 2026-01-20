using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor(
    IPacketSender packetSender,
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(packetSender, playerManager, entityRegistry)
{
    public override void Process(CreaturePoopPerformed packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}

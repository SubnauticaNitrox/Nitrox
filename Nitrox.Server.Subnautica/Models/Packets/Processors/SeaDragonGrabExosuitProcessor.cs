using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonGrabExosuitProcessor(
    IPacketSender packetSender,
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonGrabExosuit>(packetSender, playerManager, entityRegistry)
{
    public override void Process(SeaDragonGrabExosuit packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.SeaDragonId, packet.TargetId]);
}

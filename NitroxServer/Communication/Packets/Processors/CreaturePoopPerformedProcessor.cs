using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class CreaturePoopPerformedProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(playerManager, entityRegistry)
{
    public override void Process(CreaturePoopPerformed packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}

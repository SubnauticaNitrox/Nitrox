using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class CreatureActionChangedProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreatureActionChanged>(playerManager, entityRegistry)
{
    public override void Process(CreatureActionChanged packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}

using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class AggressiveWhenSeeTargetChangedProcessor : TransmitIfCanSeePacketProcessor<AggressiveWhenSeeTargetChanged>
{
    public AggressiveWhenSeeTargetChangedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : base(playerManager, entityRegistry) { }

    public override void Process(AggressiveWhenSeeTargetChanged packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId, packet.TargetId]);
}

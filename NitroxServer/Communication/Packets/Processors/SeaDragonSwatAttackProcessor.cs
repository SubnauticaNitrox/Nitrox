using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class SeaDragonSwatAttackProcessor : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>
{
    public SeaDragonSwatAttackProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : base(playerManager, entityRegistry) { }

    public override void Process(SeaDragonSwatAttack packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.SeaDragonId, packet.TargetId]);
}

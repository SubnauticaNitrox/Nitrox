using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class RangedAttackLastTargetUpdateProcessor : ClientPacketProcessor<RangedAttackLastTargetUpdate>
{
    public override void Process(RangedAttackLastTargetUpdate packet)
    {
        AI.RangedAttackLastTargetUpdate(packet.CreatureId, packet.TargetId, packet.AttackTypeIndex, packet.State);
    }
}

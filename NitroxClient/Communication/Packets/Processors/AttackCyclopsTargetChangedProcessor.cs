using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AttackCyclopsTargetChangedProcessor : ClientPacketProcessor<AttackCyclopsTargetChanged>
{
    public override void Process(AttackCyclopsTargetChanged packet)
    {
        AI.AttackCyclopsTargetChanged(packet.CreatureId, packet.TargetId, packet.AggressiveToNoiseAmount);
    }
}

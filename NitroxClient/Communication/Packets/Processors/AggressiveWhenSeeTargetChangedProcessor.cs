using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AggressiveWhenSeeTargetChangedProcessor : ClientPacketProcessor<AggressiveWhenSeeTargetChanged>
{
    public override void Process(AggressiveWhenSeeTargetChanged packet)
    {
#if SUBNAUTICA
        AI.AggressiveWhenSeeTargetChanged(packet.CreatureId, packet.TargetId, packet.Locked, packet.AggressionAmount);
#elif BELOWZERO
        AI.AggressiveWhenSeeTargetChanged(packet.CreatureId, packet.TargetId, packet.Locked, packet.AggressionAmount, packet.TargetPriority);
#endif
    }
}

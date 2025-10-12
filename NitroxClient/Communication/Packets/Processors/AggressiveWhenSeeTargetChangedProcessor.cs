using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AggressiveWhenSeeTargetChangedProcessor : ClientPacketProcessor<AggressiveWhenSeeTargetChanged>
{
    public override void Process(AggressiveWhenSeeTargetChanged packet)
    {
        AI.AggressiveWhenSeeTargetChanged(packet.CreatureId, packet.TargetId, packet.Locked, packet.AggressionAmount);
    }
}

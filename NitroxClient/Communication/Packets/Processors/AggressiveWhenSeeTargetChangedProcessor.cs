using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AggressiveWhenSeeTargetChangedProcessor : IClientPacketProcessor<AggressiveWhenSeeTargetChanged>
{
    public Task Process(IPacketProcessContext context, AggressiveWhenSeeTargetChanged packet)
    {
        AI.AggressiveWhenSeeTargetChanged(packet.CreatureId, packet.TargetId, packet.Locked, packet.AggressionAmount);
        return Task.CompletedTask;
    }
}

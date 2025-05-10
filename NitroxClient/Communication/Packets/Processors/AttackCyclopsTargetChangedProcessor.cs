using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AttackCyclopsTargetChangedProcessor : IClientPacketProcessor<AttackCyclopsTargetChanged>
{
    public Task Process(IPacketProcessContext context, AttackCyclopsTargetChanged packet)
    {
        AI.AttackCyclopsTargetChanged(packet.CreatureId, packet.TargetId, packet.AggressiveToNoiseAmount);
        return Task.CompletedTask;
    }
}

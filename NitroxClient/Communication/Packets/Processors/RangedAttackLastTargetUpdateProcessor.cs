using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class RangedAttackLastTargetUpdateProcessor : IClientPacketProcessor<RangedAttackLastTargetUpdate>
{
    public Task Process(IPacketProcessContext context, RangedAttackLastTargetUpdate packet)
    {
        AI.RangedAttackLastTargetUpdate(packet.CreatureId, packet.TargetId, packet.AttackTypeIndex, packet.State);
        return Task.CompletedTask;
    }
}

using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class RangedAttackLastTargetUpdateProcessor : IClientPacketProcessor<RangedAttackLastTargetUpdate>
{
    public Task Process(ClientProcessorContext context, RangedAttackLastTargetUpdate packet)
    {
        AI.RangedAttackLastTargetUpdate(packet.CreatureId, packet.TargetId, packet.AttackTypeIndex, packet.State);
        return Task.CompletedTask;
    }
}

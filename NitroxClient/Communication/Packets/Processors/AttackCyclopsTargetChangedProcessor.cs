using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class AttackCyclopsTargetChangedProcessor : IClientPacketProcessor<AttackCyclopsTargetChanged>
{
    public Task Process(ClientProcessorContext context, AttackCyclopsTargetChanged packet)
    {
        AI.AttackCyclopsTargetChanged(packet.CreatureId, packet.TargetId, packet.AggressiveToNoiseAmount);
        return Task.CompletedTask;
    }
}

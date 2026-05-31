using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class AggressiveWhenSeeTargetChangedProcessor : IClientPacketProcessor<AggressiveWhenSeeTargetChanged>
{
    public Task Process(ClientProcessorContext context, AggressiveWhenSeeTargetChanged packet)
    {
        AI.AggressiveWhenSeeTargetChanged(packet.CreatureId, packet.TargetId, packet.Locked, packet.AggressionAmount);
        return Task.CompletedTask;
    }
}

using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CrashAttackLastTargetProcessor : IClientPacketProcessor<CrashAttackLastTarget>
{
    public Task Process(ClientProcessorContext context, CrashAttackLastTarget packet)
    {
        AI.CrashAttackLastTarget(packet.CreatureId, packet.TargetId);
        return Task.CompletedTask;
    }
}

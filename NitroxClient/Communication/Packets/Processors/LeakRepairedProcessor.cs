using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class LeakRepairedProcessor : IClientPacketProcessor<LeakRepaired>
{
    public Task Process(ClientProcessorContext context, LeakRepaired packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.BaseId, out BaseLeakManager baseLeakManager))
        {
            baseLeakManager.HealLeakToMax(packet.RelativeCell.ToUnity());
        }
        return Task.CompletedTask;
    }
}

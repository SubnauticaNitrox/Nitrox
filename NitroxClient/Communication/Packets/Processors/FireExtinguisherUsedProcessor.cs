using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireExtinguisherUsedProcessor(FireExtinguishers fireExtinguishers) : IClientPacketProcessor<FireExtinguisherUsed>
{
    private readonly FireExtinguishers fireExtinguishers = fireExtinguishers;

    public Task Process(ClientProcessorContext context, FireExtinguisherUsed packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.Id, out FireExtinguisher fireExtinguisher))
        {
            if (packet.IsActive)
            {
                fireExtinguishers.StartUsing(packet.Id, fireExtinguisher);
            }
            else
            {
                fireExtinguishers.StopUsing(packet.Id);
            }
        }

        return Task.CompletedTask;
    }
}

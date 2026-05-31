using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class VehicleMovementsProcessor : IClientPacketProcessor<VehicleMovements>
{
    public Task Process(ClientProcessorContext context, VehicleMovements packet)
    {
        if (!MovementBroadcaster.Instance)
        {
            return Task.CompletedTask;
        }

        foreach (MovementData movementData in packet.Data)
        {
            if (MovementBroadcaster.Instance.Replicators.TryGetValue(movementData.Id, out MovementReplicator movementReplicator))
            {
                movementReplicator.AddSnapshot(movementData, (float)packet.RealTime);
            }
        }
        return Task.CompletedTask;
    }
}

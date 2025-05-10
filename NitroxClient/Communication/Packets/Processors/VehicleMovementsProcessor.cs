using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleMovementsProcessor : IClientPacketProcessor<VehicleMovements>
{
    public Task Process(IPacketProcessContext context, VehicleMovements packet)
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

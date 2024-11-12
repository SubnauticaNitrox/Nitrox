using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleMovementsProcessor : ClientPacketProcessor<VehicleMovements>
{
    public override void Process(VehicleMovements packet)
    {
        if (!MovementBroadcaster.Instance)
        {
            return;
        }

        foreach (MovementData movementData in packet.Data)
        {
            if (MovementBroadcaster.Instance.Replicators.TryGetValue(movementData.Id, out MovementReplicator movementReplicator))
            {
                movementReplicator.AddSnapshot(movementData, (float)packet.RealTime);
            }
        }
    }
}

using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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

        foreach (KeyValuePair<NitroxId, MovementData> pair in packet.Data)
        {
            if (MovementBroadcaster.Instance.Replicators.TryGetValue(pair.Key, out MovementReplicator movementReplicator))
            {
                movementReplicator.AddSnapshot(pair.Value, (float)packet.RealTime);
            }
        }
    }
}

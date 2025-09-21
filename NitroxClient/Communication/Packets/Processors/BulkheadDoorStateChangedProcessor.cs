using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

public class BulkheadDoorStateChangedProcessor : ClientPacketProcessor<BulkheadDoorStateChanged>
{
    public override void Process(BulkheadDoorStateChanged packet)
    {
        GameObject bulkheadDoor = NitroxEntity.RequireObjectFrom(packet.Id);
        if (bulkheadDoor.TryGetComponent<BulkheadDoor>(out BulkheadDoor door))
        {
            door.SetState(packet.IsOpen);
        }
    }
}

using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

public class BulkheadDoorStateChangedProcessor : ClientPacketProcessor<BulkheadDoorStateChanged>
{
    public override void Process(BulkheadDoorStateChanged packet)
    {
        Log.Info($"[Client] Received bulkhead door state change: {packet.Id} -> {(packet.IsOpen ? "OPEN" : "CLOSED")}");

        GameObject bulkheadDoor = NitroxEntity.RequireObjectFrom(packet.Id);
        if (bulkheadDoor.TryGetComponent<BulkheadDoor>(out BulkheadDoor door))
        {
            Log.Info($"[Client] Applying door state change to {door.name}");
            door.SetState(packet.IsOpen);
        }
        else
        {
            Log.Error($"[Client] Could not find BulkheadDoor component on {bulkheadDoor.name}");
        }
    }
}

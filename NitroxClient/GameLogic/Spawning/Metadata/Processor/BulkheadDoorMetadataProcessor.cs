using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class BulkheadDoorMetadataProcessor : EntityMetadataProcessor<BulkheadDoorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BulkheadDoorMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out BulkheadDoor bulkheadDoor))
        {
            bulkheadDoor = gameObject.GetComponentInChildren<BulkheadDoor>();
        }

        if (!bulkheadDoor)
        {
            Log.Warn($"[{nameof(BulkheadDoorMetadataProcessor)}] No BulkheadDoor component found on {gameObject.name} or its children");
            return;
        }

        // The state is already known by everyone, it must not be sent back
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            bulkheadDoor.SetState(metadata.Opened);
        }
    }
}

using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
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

        bulkheadDoor.SetState(metadata.Opened);
    }
}

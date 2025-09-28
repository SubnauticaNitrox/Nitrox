using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class BulkheadDoorMetadataProcessor : EntityMetadataProcessor<BulkheadDoorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BulkheadDoorMetadata metadata)
    {

        if (!gameObject.TryGetComponentInChildren<BulkheadDoor>(out BulkheadDoor door))
        {
            Log.Info("[BulkheadDoorMetadataProcessor] Unable to find BulkheadDoor");
        }

        door.SetState(metadata.IsOpen);
    }
}

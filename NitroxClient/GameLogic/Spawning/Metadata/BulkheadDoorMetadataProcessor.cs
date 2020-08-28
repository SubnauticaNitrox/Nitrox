using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class BulkheadDoorMetadataProcessor : GenericEntityMetadataProcessor<BulkheadDoorMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, BulkheadDoorMetadata metadata)
        {
            Log.Debug($"Received bulk head door metadata change for {gameObject.name} with data of {metadata}");

            BulkheadDoor bulkhead = gameObject.GetComponent<BulkheadDoor>();

            if (bulkhead)
            {
                bulkhead.SetState(metadata.IsOpen);
            }
        }
    }
}

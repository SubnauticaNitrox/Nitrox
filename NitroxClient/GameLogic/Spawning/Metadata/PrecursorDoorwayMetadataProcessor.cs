using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorDoorwayMetadataProcessor : GenericEntityMetadataProcessor<PrecursorDoorwayMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorDoorwayMetadata metadata)
        {
            Log.Info($"Received precursor door metadata change for {gameObject.name} with data of {metadata}");

            PrecursorDoorway precursorDoorway = gameObject.GetComponent<PrecursorDoorway>();
            precursorDoorway.isOpen = metadata.IsOpen;
            Log.Debug("Toggled door metadata" + metadata.IsOpen);

            if (metadata.IsOpen)
            {
                precursorDoorway.ToggleDoor(metadata.IsOpen);
                Log.Debug("Toggled door " + metadata.IsOpen);
            }
        }
    }
}

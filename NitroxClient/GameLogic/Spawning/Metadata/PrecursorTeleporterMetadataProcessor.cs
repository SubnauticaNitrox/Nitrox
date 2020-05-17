using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorTeleporterMetadataProcessor : GenericEntityMetadataProcessor<PrecursorTeleporterMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorTeleporterMetadata metadata)
        {
            Log.Info($"Received precursor teleporter metadata change for {gameObject.name} with data of {metadata}");

            PrecursorTeleporter precursorTeleporter = gameObject.GetComponent<PrecursorTeleporter>();
            precursorTeleporter.isOpen = metadata.IsOpen;

            precursorTeleporter.ToggleDoor(metadata.IsOpen);
        }
    }
}

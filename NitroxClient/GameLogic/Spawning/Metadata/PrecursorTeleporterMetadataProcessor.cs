using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorTeleporterMetadataProcessor : GenericEntityMetadataProcessor<PrecursorTeleporterMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorTeleporterMetadata metadata)
        {
            PrecursorTeleporter precursorTeleporter = gameObject.GetComponent<PrecursorTeleporter>();
            if (precursorTeleporter)
            {
                precursorTeleporter.ToggleDoor(metadata.IsOpen);
            }
        }
    }
}

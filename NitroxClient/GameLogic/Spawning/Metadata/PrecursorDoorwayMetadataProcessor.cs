using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorDoorwayMetadataProcessor : GenericEntityMetadataProcessor<PrecursorDoorwayMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorDoorwayMetadata metadata)
        {
            PrecursorDoorway precursorDoorway = gameObject.GetComponent<PrecursorDoorway>();
            precursorDoorway.isOpen = metadata.IsOpen;

            if (metadata.IsOpen)
            {
                precursorDoorway.BroadcastMessage("DisableField");
            }
            else
            {
                precursorDoorway.BroadcastMessage("EnableField");
            }
        }
    }
}

using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class PlantableMetadataProcessor : GenericEntityMetadataProcessor<PlantableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PlantableMetadata metadata)
    {
        Plantable plantable = gameObject.GetComponent<Plantable>();

        // Plantable will only have a growing plant when residing in the proper container.
        if (plantable && plantable.growingPlant)
        {
            plantable.growingPlant.SetProgress(metadata.Progress);
        }
    }
}

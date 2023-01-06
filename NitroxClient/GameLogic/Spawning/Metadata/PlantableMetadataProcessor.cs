using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class PlantableMetadataProcessor : GenericEntityMetadataProcessor<PlantableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PlantableMetadata metadata)
    {
        Plantable plantable = gameObject.GetComponent<Plantable>();

        if (plantable)
        {
            plantable.growingPlant.SetProgress(metadata.Progress);
        }
        else
        {
            Log.Error($"Could not find plantable on {gameObject.name}");
        }
    }
}

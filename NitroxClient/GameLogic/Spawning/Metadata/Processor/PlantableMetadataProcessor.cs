using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PlantableMetadataProcessor : EntityMetadataProcessor<PlantableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PlantableMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Plantable plantable))
        {
            if (plantable.growingPlant)
            {
                plantable.growingPlant.timeStartGrowth = metadata.TimeStartGrowth;
            }
            else if (plantable.model.TryGetComponent(out GrowingPlant growingPlant))
            {
                // Calculation from GrowingPlant.SetProgress (reversed because we're looking for "progress" while we already know timeStartGrowth)
                plantable.plantAge = (DayNightCycle.main.timePassedAsFloat - metadata.TimeStartGrowth) / growingPlant.GetGrowthDuration();
            }
        }
        else
        {
            Log.Error($"[{nameof(PlantableMetadataProcessor)}] Could not find {nameof(Plantable)} on {gameObject.name}");
        }
    }
}

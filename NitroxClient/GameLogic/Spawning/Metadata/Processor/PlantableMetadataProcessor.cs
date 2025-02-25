using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PlantableMetadataProcessor(FruitPlantMetadataProcessor fruitPlantMetadataProcessor) : EntityMetadataProcessor<PlantableMetadata>
{
    private readonly FruitPlantMetadataProcessor fruitPlantMetadataProcessor = fruitPlantMetadataProcessor;

    public override void ProcessMetadata(GameObject gameObject, PlantableMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out Plantable plantable))
        {
            Log.Error($"[{nameof(PlantableMetadataProcessor)}] Could not find {nameof(Plantable)} on {gameObject.name}");
            return;
        }

        if (plantable.growingPlant)
        {
            plantable.growingPlant.timeStartGrowth = metadata.TimeStartGrowth;
        }
        else if (plantable.model.TryGetComponent(out GrowingPlant growingPlant))
        {
            // Calculation from GrowingPlant.GetProgress (reversed because we're looking for "progress" while we already know timeStartGrowth)
            plantable.plantAge = Mathf.Clamp((DayNightCycle.main.timePassedAsFloat - metadata.TimeStartGrowth) / growingPlant.GetGrowthDuration(), 0f, growingPlant.maxProgress);
        }
        
        // TODO: Refer to the TODO in PlantableMetadata
        if (metadata.FruitPlantMetadata != null)
        {
            fruitPlantMetadataProcessor.ProcessMetadata(gameObject, metadata.FruitPlantMetadata);
        }
    }
}

using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FruitPlantMetadataExtractor : EntityMetadataExtractor<FruitPlant, FruitPlantMetadata>
{
    public override FruitPlantMetadata Extract(FruitPlant fruitPlant)
    {
        bool[] prefabsPicked = fruitPlant.fruits.Select(prefab => prefab.pickedState).ToArray();

        // If fruit spawn is disabled (certain plants like kelp don't regrow their fruits) and if none of the fruits were picked (all picked = false)
        // then we don't need to save this data because the plant is spawned like this by default
        if (!fruitPlant.fruitSpawnEnabled && prefabsPicked.All(b => !b))
        {
            return null;
        }

        return new(prefabsPicked, fruitPlant.timeNextFruit);
    }
}

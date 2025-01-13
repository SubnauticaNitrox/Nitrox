using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class EggMetadataExtractor : EntityMetadataExtractor<CreatureEgg, EggMetadata>
{
    public override EggMetadata Extract(CreatureEgg creatureEgg)
    {
        // If the egg is not in a water park (when being picked up or dropped outside of one),
        // we only need the exact progress value because progress only increases while inside a water park
        if (Items.PickingUpObject == creatureEgg.gameObject || !creatureEgg.insideWaterPark)
        {
            return new(-1f, creatureEgg.progress);
        }
        return new(creatureEgg.timeStartHatching, creatureEgg.progress);
    }
}

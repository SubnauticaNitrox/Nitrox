using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class WaterParkCreatureMetadataProcessor : GenericEntityMetadataProcessor<WaterParkCreatureMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, WaterParkCreatureMetadata metadata)
    {
        if (gameObject.TryGetComponent(out WaterParkCreature waterParkCreature))
        {
            if (waterParkCreature.currentWaterPark)
            {
                // MatureTime is important for fishes that are already in a WaterPark, to calculate the right age
                waterParkCreature.matureTime = metadata.MatureTime;
                double startTime = metadata.MatureTime - waterParkCreature.data.growingPeriod;
                waterParkCreature.age = Mathf.InverseLerp((float)startTime, (float)metadata.MatureTime, DayNightCycle.main.timePassedAsFloat);
                waterParkCreature.timeNextBreed = metadata.TimeNextBreed;
            }
            else
            {
                // Age is the only important constant for fishes that are in an item state
                waterParkCreature.matureTime = -1;
                waterParkCreature.age = metadata.Age;
                waterParkCreature.timeNextBreed = -1;
            }

            waterParkCreature.isMature = waterParkCreature.age == 1f;
            waterParkCreature.bornInside = metadata.BornInside;
            waterParkCreature.OnProtoDeserialize(null);
        }
    }
}

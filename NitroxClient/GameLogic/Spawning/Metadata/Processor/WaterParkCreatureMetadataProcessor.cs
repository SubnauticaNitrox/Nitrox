using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class WaterParkCreatureMetadataProcessor : EntityMetadataProcessor<WaterParkCreatureMetadata>
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

            // Scaling according to WaterParkCreature.ManagedUpdate
            waterParkCreature.transform.localScale = Mathf.Lerp(waterParkCreature.data.initialSize, waterParkCreature.data.maxSize, waterParkCreature.age) * Vector3.one;

            waterParkCreature.isMature = waterParkCreature.age == 1f;
            waterParkCreature.bornInside = metadata.BornInside;

            // This field is not serialized but is always the exact same so it's supposedly recomputed but it would break with our system
            // (calculation from WaterParkCreature.ManagedUpdate)
            waterParkCreature.breedInterval = waterParkCreature.data.growingPeriod * 0.5f;

            // While being fully loaded, the base is inactive and coroutines shouldn't be started (they'll throw an exception)
            // To avoid, that we postpone their execution to 1 more second which is enough because time is frozen during initial sync
            // This is the mating condition from WaterParkCreature.ManagedUpdate to postpone mating
            if (Multiplayer.Main && !Multiplayer.Main.InitialSyncCompleted && waterParkCreature.currentWaterPark && waterParkCreature.isMature &&
                waterParkCreature.GetCanBreed() && DayNightCycle.main.timePassedAsFloat > waterParkCreature.timeNextBreed)
            {
                waterParkCreature.timeNextBreed = DayNightCycle.main.timePassedAsFloat + 1;
            }

            waterParkCreature.OnProtoDeserialize(null);
        }
        else
        {
            Log.Error($"[{nameof(WaterParkCreatureMetadataProcessor)}] Could not find {nameof(WaterParkCreature)} on {gameObject.name}");
        }
    }
}

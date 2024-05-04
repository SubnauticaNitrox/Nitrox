using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EggMetadataProcessor : EntityMetadataProcessor<EggMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, EggMetadata metadata)
    {
        if (gameObject.TryGetComponent(out CreatureEgg creatureEgg))
        {
            if (metadata.TimeStartHatching == -1f)
            {
                // If the egg is not in a water park we only need its progress value
                creatureEgg.progress = metadata.Progress;
            }
            else
            {
                // If the egg is in a water park we only need its time start hatching value
                // the current progress will be automatically computed by UpdateProgress()
                creatureEgg.timeStartHatching = metadata.TimeStartHatching;
                
                // While being fully loaded, the base is inactive and coroutines shouldn't be started (they'll throw an exception)
                // To avoid, that we postpone their execution to 1 more second which is enough because time is frozen during initial sync
                if (Multiplayer.Main && !Multiplayer.Main.InitialSyncCompleted &&
                    creatureEgg.timeStartHatching + creatureEgg.GetHatchDuration() < DayNightCycle.main.timePassedAsFloat)
                {
                    creatureEgg.timeStartHatching = DayNightCycle.main.timePassedAsFloat + 1 - creatureEgg.GetHatchDuration();
                }

                creatureEgg.UpdateProgress();
            }
        }
        else
        {
            Log.Error($"[{nameof(EggMetadataProcessor)}] Could not find {nameof(CreatureEgg)} on {gameObject.name}");
        }
    }
}

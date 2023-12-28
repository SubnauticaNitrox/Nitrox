using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class BatteryMetadataProcessor : EntityMetadataProcessor<BatteryMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BatteryMetadata metadata)
    {
        Battery battery = gameObject.GetComponent<Battery>();

        if (battery)
        {
            battery._charge = metadata.Charge;
        }
        else
        {
            Log.Error($"Could not find Battery on {gameObject.name}");
        }
    }
}

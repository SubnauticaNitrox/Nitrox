using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class BatteryMetadataProcessor : GenericEntityMetadataProcessor<BatteryMetadata>
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

using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;
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
            DisplayStatusCode(StatusCode.subnauticaError, false);
            Log.Error($"Could not find Battery on {gameObject.name}");
        }
    }
}

using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class FireMetadataProcessor : GenericEntityMetadataProcessor<FireMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FireMetadata metadata)
    {
        Fire fire = gameObject.GetComponent<Fire>();

        if (fire)
        {
            fire.livemixin.health = metadata.Health;

            if (!fire.livemixin.IsAlive() && !fire.isExtinguished)
            {
                fire.Extinguished();
            }
        }
        else
        {
            Log.Error($"Could not find Fire on {gameObject.name}");
        }
    }
}

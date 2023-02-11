using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class FireExtinguisherHolderMetadataProcessor : GenericEntityMetadataProcessor<FireExtinguisherHolderMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FireExtinguisherHolderMetadata metadata)
    {
        FireExtinguisherHolder holder = gameObject.GetComponent<FireExtinguisherHolder>();

        if (holder)
        {
            holder.fuel = metadata.Fuel;
            holder.hasTank = metadata.HasExtinguisher;
            holder.tankObject.SetActive(metadata.HasExtinguisher);
        }
        else
        {
            Log.Error($"Could not find FireExtinguisherHolder on {gameObject.name}");
        }
    }
}

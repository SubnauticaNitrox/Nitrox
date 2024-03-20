using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class FireExtinguisherHolderMetadataProcessor : EntityMetadataProcessor<FireExtinguisherHolderMetadata>
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
            DisplayStatusCode(StatusCode.SUBNAUTICA_ERROR, $"Could not find FireExtinguisherHolder on {gameObject.name}");
        }
    }
}

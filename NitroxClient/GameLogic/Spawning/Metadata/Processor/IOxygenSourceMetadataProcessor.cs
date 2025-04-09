using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class IOxygenSourceMetadataProcessor : EntityMetadataProcessor<IOxygenSourceMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, IOxygenSourceMetadata metadata)
    {
        IOxygenSource iOxygenSource = gameObject.GetComponent<IOxygenSource>();

        if (iOxygenSource != null)
        {
            iOxygenSource.AddOxygen(metadata.AvailableOxygen);
        }
        else
        {
            Log.Error($"Could not find IOxygenSource on {gameObject.name}");
        }
    }
}

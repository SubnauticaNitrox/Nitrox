using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class ConstructorMetadataProcessor : EntityMetadataProcessor<ConstructorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, ConstructorMetadata metadata)
    {
        Constructor constructor = gameObject.GetComponent<Constructor>();

        if (constructor)
        {
            constructor.Deploy(metadata.Deployed);
        }
        else
        {
            Log.Error($"Could not find constructor on {gameObject.name}");
        }
    }
}

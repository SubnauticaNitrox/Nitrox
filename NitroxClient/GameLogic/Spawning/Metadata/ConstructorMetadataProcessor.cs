using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class ConstructorMetadataProcessor : GenericEntityMetadataProcessor<ConstructorMetadata>
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

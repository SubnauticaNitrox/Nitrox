using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MetadataHolder : MonoBehaviour
{
    public EntityMetadata Metadata;

    public EntityMetadata Consume()
    {
        Destroy(this);
        return Metadata;
    }

    public static MetadataHolder AddMetadata(GameObject gameObject, EntityMetadata metadata)
    {
        MetadataHolder metadataHolder = gameObject.AddComponent<MetadataHolder>();
        metadataHolder.Metadata = metadata;
        return metadataHolder;
    }
}

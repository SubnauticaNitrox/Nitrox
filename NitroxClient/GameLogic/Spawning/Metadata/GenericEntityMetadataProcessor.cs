using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public abstract class GenericEntityMetadataProcessor<T> : EntityMetadataProcessor where T : EntityMetadata
    {
        public abstract void ProcessMetadata(GameObject gameObject, T metadata);

        public override void ProcessMetadata(GameObject gameObject, EntityMetadata metadata)
        {
            ProcessMetadata(gameObject, (T)metadata);
        }
    }
}

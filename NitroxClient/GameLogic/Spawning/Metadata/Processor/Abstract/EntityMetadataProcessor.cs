using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;

public abstract class EntityMetadataProcessor<TMetadata> : IEntityMetadataProcessor where TMetadata : EntityMetadata
{
    public abstract void ProcessMetadata(GameObject gameObject, TMetadata metadata);

    public void ProcessMetadata(GameObject gameObject, EntityMetadata metadata)
    {
        ProcessMetadata(gameObject, (TMetadata)metadata);
    }

    protected TService Resolve<TService>() where TService : class
    {
        return NitroxServiceLocator.Cache<TService>.Value;
    }
}

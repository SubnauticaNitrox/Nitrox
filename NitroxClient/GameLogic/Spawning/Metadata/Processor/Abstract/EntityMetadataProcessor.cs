using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;

public abstract class EntityMetadataProcessor<T> : IEntityMetadataProcessor where T : EntityMetadata
{
    public abstract void ProcessMetadata(GameObject gameObject, T metadata);

    public void ProcessMetadata(GameObject gameObject, EntityMetadata metadata)
    {
        ProcessMetadata(gameObject, (T)metadata);
    }

    protected TService Resolve<TService>() where TService : class
    {
        return NitroxServiceLocator.Cache<TService>.Value;
    }
}

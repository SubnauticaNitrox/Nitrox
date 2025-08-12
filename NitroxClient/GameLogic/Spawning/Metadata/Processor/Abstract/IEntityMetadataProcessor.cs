using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;

public interface IEntityMetadataProcessor
{
    public abstract void ProcessMetadata(GameObject gameObject, EntityMetadata metadata);
}

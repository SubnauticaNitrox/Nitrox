using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    /**
     * Some entities may already exist in the world but the server knows about them (an example being
     * a server spawned prefab that had a lot of children game objects backed into it).  We don't want
     * to respawn these objects; instead, just update the nitrox id and apply any entity metadata.
     */
    public class ExistingGameObjectSpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
        {
            if (!parent.HasValue)
            {
                return Optional.Empty;
            }

            if (parent.Value.transform.childCount - 1 < entity.ExistingGameObjectChildIndex.Value)
            {
                Log.Error($"Parent {parent.Value} did not have a child at index {entity.ExistingGameObjectChildIndex.Value}");
                return Optional.Empty;
            }
            
            GameObject gameObject = parent.Value.transform.GetChild(entity.ExistingGameObjectChildIndex.Value).gameObject;
            
            NitroxEntity.SetNewId(gameObject, entity.Id);

            Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

            if (metadataProcessor.HasValue)
            {
                metadataProcessor.Value.ProcessMetadata(gameObject, entity.Metadata);
            }
                    
            return Optional.Of(gameObject);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

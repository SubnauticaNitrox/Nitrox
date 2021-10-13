using System.Collections;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
#if SUBNAUTICA
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#elif BELOWZERO
        public IEnumerator Spawn(TaskResult<Optional<GameObject>> result, Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#endif
        {
            TechType techType = entity.TechType.ToUnity();
#if SUBNAUTICA
            GameObject gameObject = CreateGameObject(techType, entity.ClassId);
#elif BELOWZERO
            TaskResult<GameObject> gameObjectResult = new TaskResult<GameObject>();
            yield return CreateGameObject(gameObjectResult, techType, entity.ClassId);

            GameObject gameObject = gameObjectResult.Get();
#endif
            gameObject.transform.position = entity.Transform.Position.ToUnity();
            gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
            gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

            NitroxEntity.SetNewId(gameObject, entity.Id);
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            if (parent.HasValue && !parent.Value.GetComponent<LargeWorldEntityCell>())
            {
                LargeWorldEntity.Register(gameObject); // This calls SetActive on the GameObject
            }
            else if (gameObject.GetComponent<LargeWorldEntity>() != null && gameObject.transform.parent == null)
            {
                gameObject.transform.SetParent(cellRoot.liveRoot.transform, true);
                LargeWorldEntity.Register(gameObject);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (parent.HasValue)
            {
                gameObject.transform.SetParent(parent.Value.transform, true);
            }

            Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

            if (metadataProcessor.HasValue)
            {
                metadataProcessor.Value.ProcessMetadata(gameObject, entity.Metadata);
            }
#if SUBNAUTICA
            return Optional.Of(gameObject);
#elif BELOWZERO
            result.Set(Optional.Of(gameObject));
            yield break;
#endif
        }

#if SUBNAUTICA
        private GameObject CreateGameObject(TechType techType, string classId)
#elif BELOWZERO
        private IEnumerator CreateGameObject(TaskResult<GameObject> result, TechType techType, string classId)
#endif
        {
            GameObject prefab = null;
            if (PrefabDatabase.TryGetPrefabFilename(classId, out string filename))
            {
                prefab = Resources.Load<GameObject>(filename);
            }

            if (prefab == null)
            {
                prefab = CraftData.GetPrefabForTechType(techType, false);
                if (prefab == null)
                {
#if SUBNAUTICA
                    return Utils.CreateGenericLoot(techType);
#elif BELOWZERO
                    result.Set(Utils.CreateGenericLoot(techType));
#endif
                }
            }
#if SUBNAUTICA
            return Utils.SpawnFromPrefab(prefab, null);
#elif BELOWZERO
            result.Set(Utils.SpawnFromPrefab(prefab, null));
#endif
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

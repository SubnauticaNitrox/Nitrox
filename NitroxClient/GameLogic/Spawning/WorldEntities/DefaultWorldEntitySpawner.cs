using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class DefaultWorldEntitySpawner : IWorldEntitySpawner
    {
        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            TechType techType = entity.TechType.ToUnity();

            TaskResult<GameObject> gameObjectResult = new();
            yield return CreateGameObject(techType, entity.ClassId, gameObjectResult);

            GameObject gameObject = gameObjectResult.Get();
            gameObject.transform.position = entity.Transform.Position.ToUnity();
            gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
            gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

            NitroxEntity.SetNewId(gameObject, entity.Id);
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            if (parent.HasValue && !parent.Value.GetComponent<LargeWorldEntityCell>())
            {
                LargeWorldEntity.Register(gameObject); // This calls SetActive on the GameObject
            }
            else if (gameObject.GetComponent<LargeWorldEntity>() && !gameObject.transform.parent && cellRoot.liveRoot)
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

            result.Set(Optional.Of(gameObject));
        }

        public static IEnumerator CreateGameObject(TechType techType, string classId, TaskResult<GameObject> result)
        {
            IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(classId);
            yield return prefabCoroutine;
            prefabCoroutine.TryGetPrefab(out GameObject prefab);

            if (!prefab)
            {
                CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(techType, false);
                yield return techPrefabCoroutine;
                prefab = techPrefabCoroutine.GetResult();
                if (!prefab)
                {
                    result.Set(Utils.CreateGenericLoot(techType));
                }
            }

            result.Set(Utils.SpawnFromPrefab(prefab, null));
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

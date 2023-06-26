using System.Collections;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class DefaultWorldEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
    {
        private static Dictionary<TechType, GameObject> prefabCacheByTechType = new();
        private static Dictionary<string, GameObject> prefabCacheByClassId = new();
        private static HashSet<(string, TechType)> prefabNotFound = new();

        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            TechType techType = entity.TechType.ToUnity();

            TaskResult<GameObject> gameObjectResult = new();
            yield return CreateGameObject(techType, entity.ClassId, gameObjectResult);

            GameObject gameObject = gameObjectResult.Get();
            SetupObject(entity, parent, gameObject, cellRoot, techType);

            result.Set(Optional.Of(gameObject));
        }

        private void SetupObject(WorldEntity entity, Optional<GameObject> parent, GameObject gameObject, EntityCell cellRoot, TechType techType)
        {
            gameObject.transform.position = entity.Transform.Position.ToUnity();
            gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
            gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

            NitroxEntity.SetNewId(gameObject, entity.Id);
            CrafterLogic.NotifyCraftEnd(gameObject, techType);

            WaterPark parentWaterPark = parent.HasValue ? parent.Value.GetComponent<WaterPark>() : null;
            if (!parentWaterPark)
            {
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
            }

            if (parent.HasValue)
            {
                if (parentWaterPark && gameObject.TryGetComponent(out Pickupable pickupable))
                {
                    pickupable.SetVisible(false);
                    pickupable.Activate(false);
                    parentWaterPark.AddItem(pickupable);
                }
                else
                {
                    gameObject.transform.SetParent(parent.Value.transform, true);
                }
            }
        }

        public static bool TryGetCachedPrefab(out GameObject prefab, TechType techType = TechType.None, string classId = null)
        {
            if ((classId != null && prefabCacheByClassId.TryGetValue(classId, out prefab)) ||
                (techType != TechType.None && prefabCacheByTechType.TryGetValue(techType, out prefab)))
            {
                return true;
            }
            prefab = null;
            return false;
        }

        public static IEnumerator RequestPrefab(TechType techType, TaskResult<GameObject> result)
        {
            if (prefabCacheByTechType.TryGetValue(techType, out GameObject prefab))
            {
                result.Set(prefab);
                yield break;
            }
            CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return techPrefabCoroutine;
            prefabCacheByTechType[techType] = techPrefabCoroutine.GetResult();
            result.Set(techPrefabCoroutine.GetResult());
        }

        public static IEnumerator CreateGameObject(TechType techType, string classId, TaskResult<GameObject> result)
        {
            IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(classId);
            yield return prefabCoroutine;
            prefabCoroutine.TryGetPrefab(out GameObject prefab);
            if (prefab)
            {
                prefabCacheByClassId[classId] = prefab;
            }

            if (!prefab)
            {
                CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(techType, false);
                yield return techPrefabCoroutine;
                prefab = techPrefabCoroutine.GetResult();
                if (!prefab)
                {
                    result.Set(Utils.CreateGenericLoot(techType));
                    prefabNotFound.Add((classId, techType));
                }
                else
                {
                    prefabCacheByTechType[techType] = prefab;
                }
            }

            result.Set(Utils.SpawnFromPrefab(prefab, null));
        }

        public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            TechType techType = entity.TechType.ToUnity();
            
            if (prefabNotFound.Contains((entity.ClassId, techType)))
            {
                SetupObject(entity, parent, Utils.CreateGenericLoot(techType), cellRoot, techType);
                return true;
            }
            else if (TryGetCachedPrefab(out GameObject prefab, techType, entity.ClassId))
            {
                SetupObject(entity, parent, Utils.SpawnFromPrefab(prefab, null), cellRoot, techType);
                return true;
            }
            return false;
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

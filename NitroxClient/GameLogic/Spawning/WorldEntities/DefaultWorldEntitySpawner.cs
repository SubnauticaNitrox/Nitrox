using System.Collections;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;
using static NitroxClient.Unity.Helper.GameObjectHelper;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class DefaultWorldEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private static readonly Dictionary<TechType, GameObject> prefabCacheByTechType = [];
    private static readonly Dictionary<string, GameObject> prefabCacheByClassId = [];
    private static readonly HashSet<(string, TechType)> prefabNotFound = [];
    private static readonly HashSet<string> classIdsWithoutPrefab = [];

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        TechType techType = entity.TechType.ToUnity();

        TaskResult<GameObject> gameObjectResult = new();
        yield return CreateGameObject(techType, entity.ClassId, entity.Id, gameObjectResult);

        GameObject gameObject = gameObjectResult.Get();
        SetupObject(entity, parent, gameObject, cellRoot, techType);

        result.Set(Optional.Of(gameObject));
    }

    private void SetupObject(WorldEntity entity, Optional<GameObject> parent, GameObject gameObject, EntityCell cellRoot, TechType techType)
    {
        gameObject.transform.position = entity.Transform.Position.ToUnity();
        gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        CrafterLogic.NotifyCraftEnd(gameObject, techType);

        WaterPark parentWaterPark = null;
        if (parent.HasValue)
        {
            Items.TryGetParentWaterPark(parent.Value.transform.parent, out parentWaterPark);
        }

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
        if (classId != null && prefabCacheByClassId.TryGetValue(classId, out prefab))
        {
            return true;
        }

        // If we've never even once issued a request prefab for the class id we need to do it because multiple prefabs
        // can have the same TechType so it's not good enough to find the right prefab
        if (!classIdsWithoutPrefab.Contains(classId) || techType == TechType.None)
        {
            prefab = null;
            return false;
        }
        
        return prefabCacheByTechType.TryGetValue(techType, out prefab);
    }

    /// <summary>
    /// Either gets the prefab reference from the cache or requests it and fills the provided result with it.
    /// </summary>
    /// <remarks>
    /// <see cref="PrefabDatabase"/> requires executing an extra yield instruction which is avoided here.
    /// Because each yield costs a non-required time (and non-neglectable considering the amount of entities) for batch spawning.
    /// Pumping a coroutine isn't possible when it contains prefab loading instructions as the one used here.
    /// </remarks>
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

    /// <inheritdoc cref="RequestPrefab(TechType, TaskResult{GameObject})"/>
    public static IEnumerator RequestPrefab(string classId, TaskResult<GameObject> result)
    {
        if (prefabCacheByClassId.TryGetValue(classId, out GameObject prefab))
        {
            result.Set(prefab);
            yield break;
        }
        IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(classId);
        yield return prefabCoroutine;
        if (prefabCoroutine.TryGetPrefab(out prefab))
        {
            prefabCacheByClassId[classId] = prefab;
        }
        result.Set(prefab);
    }

    public static IEnumerator CreateGameObject(TechType techType, string classId, NitroxId nitroxId, TaskResult<GameObject> result)
    {
        IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(classId);
        yield return prefabCoroutine;
        if (prefabCoroutine.TryGetPrefab(out GameObject prefab))
        {
            prefabCacheByClassId[classId] = prefab;
        }
        else
        {
            classIdsWithoutPrefab.Add(classId);
            CoroutineTask<GameObject> techPrefabCoroutine = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return techPrefabCoroutine;
            prefab = techPrefabCoroutine.GetResult();
            if (!prefab)
            {
                result.Set(CreateGenericLoot(techType, nitroxId));
                prefabNotFound.Add((classId, techType));
                yield break;
            }
            else
            {
                prefabCacheByTechType[techType] = prefab;
            }
        }

        result.Set(SpawnFromPrefab(prefab, nitroxId));
    }

    /// <summary>
    /// Looks in prefab cache and creates a GameObject out of it if possible, or returns false.
    /// </summary>
    public static bool TryCreateGameObjectSync(TechType techType, string classId, NitroxId nitroxId, out GameObject gameObject)
    {
        if (prefabNotFound.Contains((classId, techType)))
        {
            gameObject = CreateGenericLoot(techType, nitroxId);
            return true;
        }
        else if (TryGetCachedPrefab(out GameObject prefab, techType, classId))
        {
            gameObject = SpawnFromPrefab(prefab, nitroxId);
            return true;
        }
        gameObject = null;
        return false;
    }

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        TechType techType = entity.TechType.ToUnity();

        if (TryCreateGameObjectSync(techType, entity.ClassId, entity.Id, out GameObject gameObject))
        {
            SetupObject(entity, parent, gameObject, cellRoot, techType);
            result.Set(gameObject);
            return true;
        }

        return false;
    }

    public bool SpawnsOwnChildren() => false;
}

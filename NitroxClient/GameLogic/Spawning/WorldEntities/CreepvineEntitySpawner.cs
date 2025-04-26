using System.Collections;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class CreepvineEntitySpawner(DefaultWorldEntitySpawner defaultWorldEntitySpawner) : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private readonly DefaultWorldEntitySpawner defaultWorldEntitySpawner = defaultWorldEntitySpawner;

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        yield return defaultWorldEntitySpawner.SpawnAsync(entity, parent, cellRoot, result);
        if (!result.value.HasValue)
        {
            yield break;
        }

        SetupObject(result.value.Value);

        // result is already set by defaultWorldEntitySpawner.SpawnAsync
    }

    public bool SpawnsOwnChildren() => false;

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (!defaultWorldEntitySpawner.SpawnSync(entity, parent, cellRoot, result))
        {
            return false;
        }

        SetupObject(result.value.Value);

        // result is already set
        return true;
    }

    private static void SetupObject(GameObject gameObject)
    {
        if (gameObject.GetComponent<FruitPlant>())
        {
            return;
        }

        FruitPlant fruitPlant = gameObject.AddComponent<FruitPlant>();
        fruitPlant.fruitSpawnEnabled = false;
        fruitPlant.timeNextFruit = -1;
        fruitPlant.fruits = gameObject.GetComponentsInChildren<PickPrefab>(true);
    }
}

using System;
using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class ReefbackEntitySpawner(ReefbackChildEntitySpawner reefbackChildEntitySpawner, DefaultWorldEntitySpawner defaultWorldEntitySpawner, Entities entities) : IWorldEntitySpawner
{
    private readonly ReefbackChildEntitySpawner reefbackChildEntitySpawner = reefbackChildEntitySpawner;
    private readonly DefaultWorldEntitySpawner defaultWorldEntitySpawner = defaultWorldEntitySpawner;
    private readonly Entities entities = entities;

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not ReefbackEntity reefbackEntity)
        {
            Log.Error($"[{nameof(ReefbackEntitySpawner)}] Can't spawn {entity.Id} of type {entity.GetType()} because it is not a {nameof(ReefbackEntity)}");
            yield break;
        }

        if (!DefaultWorldEntitySpawner.TryCreateGameObjectSync(entity.TechType.ToUnity(), entity.ClassId, entity.Id, out GameObject reefbackObject))
        {
            Log.ErrorOnce($"[{nameof(PlaceholderGroupWorldEntitySpawner)}] Could not find a prefab for {entity.Id} [classId: {entity.ClassId}, TechType: {entity.TechType}]");
            yield break;
        }
        ReefbackLife reefbackLife = reefbackObject.GetComponent<ReefbackLife>();
        LargeWorldEntity largeWorldEntity = reefbackObject.GetComponent<LargeWorldEntity>();


        // Prevent the entity from disappearing because of parent cell going to sleep until it's fully spawned
        largeWorldEntity.enabled = false;

        SetupObject(reefbackEntity, reefbackObject, cellRoot, reefbackLife);

        TaskResult<Optional<GameObject>> childTaskResult = new();
        foreach (ReefbackChildEntity reefbackChildEntity in entity.ChildEntities)
        {
            reefbackChildEntitySpawner.SpawnSync(reefbackChildEntity, reefbackObject, cellRoot, childTaskResult);

            if (childTaskResult.Get().HasValue)
            {
                entities.OnEntitySpawned(reefbackChildEntity, childTaskResult.Get().Value);
            }

            if (entities.ShouldSkipFrame)
            {
                yield return null;
                entities.RefreshTimeUntilNextYield();
            }
        }

        largeWorldEntity.enabled = true;
        LargeWorldEntity.Register(reefbackObject);

        result.Set(reefbackObject);
    }

    public bool SpawnsOwnChildren() => true;

    private void SetupObject(ReefbackEntity entity, GameObject gameObject, EntityCell cellRoot, ReefbackLife reefbackLife)
    {
        defaultWorldEntitySpawner.SetupObject(entity, Optional.Empty, gameObject, cellRoot, entity.TechType.ToUnity(), false);

        // Replicate only the useful parts of ReefbackLife.Initialize
        reefbackLife.initialized = true;
        reefbackLife.needToRemovePlantPhysics = false;
        reefbackLife.hasCorals = gameObject.transform.localScale.x > 0.8f;

        if (reefbackLife.hasCorals && LargeWorld.main)
        {
            string biome = LargeWorld.main.GetBiome(entity.OriginalPosition.ToUnity());
            if (!string.IsNullOrEmpty(biome) && biome.StartsWith("grassyplateaus", StringComparison.OrdinalIgnoreCase))
            {
                reefbackLife.grassIndex = 0;
            }
            else
            {
                reefbackLife.grassIndex = entity.GrassIndex;
            }
        }

        // Only useful stuff from ReefbackLife.CoSpawn
        reefbackLife.corals.SetActive(reefbackLife.hasCorals);
        reefbackLife.islands.SetActive(reefbackLife.hasCorals);
        if (reefbackLife.grassIndex >= 0 && reefbackLife.grassIndex < reefbackLife.grassVariants.Length)
        {
            reefbackLife.grassVariants[reefbackLife.grassIndex].SetActive(true);
        }
    }
}

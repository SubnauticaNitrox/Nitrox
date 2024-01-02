using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class ReefbackWorldEntitySpawner : IWorldEntitySpawner
    {
        private readonly DefaultWorldEntitySpawner defaultSpawner;

        public ReefbackWorldEntitySpawner(DefaultWorldEntitySpawner defaultSpawner)
        {
            this.defaultSpawner = defaultSpawner;
        }

        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            TaskResult<Optional<GameObject>> reefbackTaskResult = new();
            yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, reefbackTaskResult);
            Optional<GameObject> reefback = reefbackTaskResult.Get();
            if (!reefback.HasValue)
            {
                result.Set(Optional.Empty);
                yield break;
            }

            if (!reefback.Value.TryGetComponent(out ReefbackLife life))
            {
                result.Set(Optional.Empty);
                yield break;
            }

            life.initialized = true;
            life.SpawnPlants();
            foreach (Entity childEntity in entity.ChildEntities)
            {
                if (childEntity is WorldEntity worldChild)
                {
                    TaskResult<Optional<GameObject>> childTaskResult = new();
                    yield return defaultSpawner.SpawnAsync(worldChild, reefback, cellRoot, childTaskResult);
                    Optional<GameObject> child = childTaskResult.Get();

                    if (child.HasValue)
                    {
                        child.Value.AddComponent<ReefbackCreature>();
                    }
                }
            }

            result.Set(Optional.Empty);
            yield break;
        }

        public bool SpawnsOwnChildren()
        {
            return true;
        }
    }
}

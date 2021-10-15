using System.Collections;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class ReefbackEntitySpawner : IEntitySpawner
    {
        private readonly DefaultEntitySpawner defaultSpawner;

        public ReefbackEntitySpawner(DefaultEntitySpawner defaultSpawner)
        {
            this.defaultSpawner = defaultSpawner;
        }

        public IEnumerator Spawn(TaskResult<Optional<GameObject>> result, Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
        {
            TaskResult<Optional<GameObject>> reefbackTaskResult = new();
            yield return defaultSpawner.Spawn(reefbackTaskResult, entity, parent, cellRoot);

            Optional<GameObject> reefback = reefbackTaskResult.Get();
            if (!reefback.HasValue)
            {
                result.Set(Optional.Empty);
                yield break;
            }
            ReefbackLife life = reefback.Value.GetComponent<ReefbackLife>();
            if (life == null)
            {
                result.Set(Optional.Empty);
                yield break;
            }

            life.initialized = true;
            life.ReflectionCall("SpawnPlants");
            foreach (Entity childEntity in entity.ChildEntities)
            {
                TaskResult<Optional<GameObject>> childTaskResult = new();
                yield return defaultSpawner.Spawn(childTaskResult, childEntity, reefback, cellRoot);

                Optional<GameObject> child = childTaskResult.Get();
                if (child.HasValue)
                {
                    child.Value.AddComponent<ReefbackCreature>();
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

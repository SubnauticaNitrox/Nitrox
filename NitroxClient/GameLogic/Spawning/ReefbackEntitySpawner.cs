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

#if SUBNAUTICA
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#elif BELOWZERO
        public IEnumerator Spawn(TaskResult<Optional<GameObject>> result, Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#endif
        {
#if SUBNAUTICA
            Optional<GameObject> reefback = defaultSpawner.Spawn(entity, parent, cellRoot);
#elif BELOWZERO
            TaskResult<Optional<GameObject>> reefbackTaskResult = new();
            yield return defaultSpawner.Spawn(reefbackTaskResult, entity, parent, cellRoot);

            Optional<GameObject> reefback = reefbackTaskResult.Get();
#endif
            if (!reefback.HasValue)
            {
#if SUBNAUTICA
                return Optional.Empty;
#elif BELOWZERO
                result.Set(Optional.Empty);
                yield break;
#endif
            }
            ReefbackLife life = reefback.Value.GetComponent<ReefbackLife>();
            if (life == null)
            {
#if SUBNAUTICA
                return Optional.Empty;
#elif BELOWZERO
                result.Set(Optional.Empty);
                yield break;
#endif
            }

            life.initialized = true;
            life.ReflectionCall("SpawnPlants");
            foreach (Entity childEntity in entity.ChildEntities)
            {
#if SUBNAUTICA
                Optional<GameObject> child = defaultSpawner.Spawn(childEntity, reefback, cellRoot);
#elif BELOWZERO
                TaskResult<Optional<GameObject>> childTaskResult = new();
                yield return defaultSpawner.Spawn(childTaskResult, childEntity, reefback, cellRoot);

                Optional<GameObject> child = childTaskResult.Get();
#endif
                if (child.HasValue)
                {
                    child.Value.AddComponent<ReefbackCreature>();
                }
            }

#if SUBNAUTICA
                return Optional.Empty;
#elif BELOWZERO
            result.Set(Optional.Empty);
            yield break;
#endif
        }

        public bool SpawnsOwnChildren()
        {
            return true;
        }
    }
}

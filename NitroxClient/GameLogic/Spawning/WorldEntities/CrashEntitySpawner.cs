using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class CrashEntitySpawner : IWorldEntitySpawner
    {
        /**
         * Crash fish are spawned by the CrashHome in the Monobehaviours Start method
         */
        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            if (parent.HasValue)
            {
                CrashHome crashHome = parent.Value.GetComponent<CrashHome>();

                GameObject gameObject = Object.Instantiate(crashHome.crashPrefab, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
                gameObject.transform.SetParent(crashHome.transform, false);
                crashHome.crash = gameObject.GetComponent<Crash>();
                crashHome.spawnTime = -1;

                NitroxEntity.SetNewId(gameObject, entity.Id);
            }

            result.Set(Optional.Empty);
            yield break;
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

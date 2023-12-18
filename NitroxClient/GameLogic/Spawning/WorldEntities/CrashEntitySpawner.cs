using System.Collections;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    // TODO: Fix all of this
    public class CrashEntitySpawner : IWorldEntitySpawner
    {
        private static readonly Quaternion spawnRotation = Quaternion.Euler(-90f, 0f, 0f);

        /**
         * Crash fish are spawned by the CrashHome in the Monobehaviours Start method
         */
        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            if (parent.HasValue)
            {
                CrashHome crashHome = parent.Value.GetComponent<CrashHome>();

                GameObject gameObject = GameObjectHelper.InstantiateWithId(crashHome.crashPrefab, entity.Id, rotation: spawnRotation);
                gameObject.transform.SetParent(crashHome.transform, false);
                crashHome.crash = gameObject.GetComponent<Crash>();
                crashHome.spawnTime = -1;
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

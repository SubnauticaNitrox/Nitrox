using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class CrashEntitySpawner : IEntitySpawner
    {
        /**
         * Crash fish are spawned by the CrashHome in the Monobehaviours Start method
         */
#if SUBNAUTICA
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#elif BELOWZERO
        public IEnumerator Spawn(TaskResult<Optional<GameObject>> result, Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
#endif
        {
            if (parent.HasValue)
            {
                CrashHome crashHome = parent.Value.GetComponent<CrashHome>();

                GameObject gameObject = Object.Instantiate(crashHome.crashPrefab, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
                gameObject.transform.SetParent(crashHome.transform, false);
                NitroxEntity.SetNewId(gameObject, entity.Id);
                crashHome.ReflectionSet("crash", gameObject.GetComponent<Crash>());
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
            return false;
        }
    }
}

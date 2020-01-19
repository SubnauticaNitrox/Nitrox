using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            if (parent.IsPresent())
            {
                CrashHome crashHome = parent.Get().GetComponent<CrashHome>();

                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(crashHome.crashPrefab, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
                gameObject.transform.SetParent(crashHome.transform, false);
                NitroxIdentifier.SetNewId(gameObject, entity.Id);
                ReflectionHelper.ReflectionSet<CrashHome>(crashHome, "crash", gameObject.GetComponent<Crash>());
            }

            return Optional<GameObject>.Empty();
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

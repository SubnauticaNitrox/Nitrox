﻿using System.Collections;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
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
                LargeWorldStreamer.main.StartCoroutine(WaitToAssignGuid(entity.Guid, crashHome));
            }

            return Optional<GameObject>.Empty();
        }

        /**
         * Wait for some time so the CrashHome can load and spawn the Crash fish.
         * If we try to manually spawn the crash fish (and assign to the CrashHome) it will be at
         * the wrong orientation.  Maybe someone can figure out why this happens to we can create 
         * it without leveraging this hack.
         */
        private IEnumerator WaitToAssignGuid(string guid, CrashHome crashHome)
        {
            yield return new WaitForSeconds(0.25f); 
            ((Crash)crashHome.ReflectionGet("crash")).gameObject.SetNewGuid(guid);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}

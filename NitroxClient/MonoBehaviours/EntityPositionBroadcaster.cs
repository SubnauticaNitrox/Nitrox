using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class EntityPositionBroadcaster : MonoBehaviour
    {
        private static Dictionary<string, GameObject> watchingEntitiesByGuid = new Dictionary<string, GameObject>();
        private Entities entityBroadcaster;

        private float time;
        private float interpolationPeriod = 0.25f;

        public void Awake()
        {
            entityBroadcaster = NitroxServiceLocator.LocateService<Entities>();
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                time = 0;

                if (watchingEntitiesByGuid.Count > 0)
                {
                    entityBroadcaster.BroadcastTransforms(watchingEntitiesByGuid);
                }
            }
        }

        public static void WatchEntity(string guid, GameObject gameObject)
        {
            watchingEntitiesByGuid[guid] = gameObject;
        }

        public static void StopWatchingEntity(string guid)
        {
            watchingEntitiesByGuid.Remove(guid);
        }
    }
}

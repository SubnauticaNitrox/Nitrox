using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class EntityPositionBroadcaster : MonoBehaviour
    {
        private static Dictionary<string, GameObject> watchingEntitiesByGuid = new Dictionary<string, GameObject>();

        private float time = 0.0f;
        private float interpolationPeriod = 0.25f;

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                time = 0;
                
                Multiplayer.Logic.Entities.BroadcastTransforms(watchingEntitiesByGuid);
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

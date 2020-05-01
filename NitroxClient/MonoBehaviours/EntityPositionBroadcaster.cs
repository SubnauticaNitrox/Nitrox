using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class EntityPositionBroadcaster : MonoBehaviour
    {
        public static readonly float BROADCAST_INTERVAL = 0.25f;

        private static Dictionary<NitroxId, GameObject> watchingEntitiesById = new Dictionary<NitroxId, GameObject>();
        private Entities entityBroadcaster;

        private float time;

        public void Awake()
        {
            entityBroadcaster = NitroxServiceLocator.LocateService<Entities>();
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                if (watchingEntitiesById.Count > 0)
                {
                    entityBroadcaster.BroadcastTransforms(watchingEntitiesById);
                }
            }
        }

        public static void WatchEntity(NitroxId id, GameObject gameObject)
        {
            watchingEntitiesById[id] = gameObject;
            
            RemotelyControlled remotelyControlled = gameObject.GetComponent<RemotelyControlled>();
            Object.Destroy(remotelyControlled);
        }

        public static void StopWatchingEntity(NitroxId id)
        {
            watchingEntitiesById.Remove(id);
        }
    }
}

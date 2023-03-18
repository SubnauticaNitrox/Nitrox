using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class EntityPositionBroadcaster : MonoBehaviour
{
    public static readonly float BROADCAST_INTERVAL = 0.25f;

    private static HashSet<NitroxId> watchingEntityIds = new();
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

            if (watchingEntityIds.Count > 0)
            {
                Dictionary<NitroxId, GameObject> gameObjectsById = NitroxEntity.GetObjectsFrom(watchingEntityIds);
                entityBroadcaster.BroadcastTransforms(gameObjectsById);
            }
        }
    }

    public static void WatchEntity(NitroxId id)
    {
        watchingEntityIds.Add(id);

        // The game object may not exist at this very moment (due to being spawned in async). This is OK as we will
        // automatically start sending updates when we finally get it in the world. This behavior will also allow us
        // to resync or respawn entities while still have broadcasting enabled without doing anything extra.
        Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(id);

        if (gameObject.HasValue)
        {
            RemotelyControlled remotelyControlled = gameObject.Value.GetComponent<RemotelyControlled>();
            Object.Destroy(remotelyControlled);
        }
    }

    public static void StopWatchingEntity(NitroxId id)
    {
        watchingEntityIds.Remove(id);
    }
}

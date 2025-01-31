using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxModel.Packets.EntityTransformUpdates;

namespace NitroxClient.MonoBehaviours;

public class EntityPositionBroadcaster : MonoBehaviour
{
    public static readonly float BROADCAST_INTERVAL = 0.25f;

    private static HashSet<NitroxId> watchingEntityIds = new();

    private static Dictionary<NitroxId, SplineTransformUpdate> splineUpdatesById = new();

    private IPacketSender packetSender;

    private float time;

    public void Awake()
    {
        packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
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
                Dictionary<NitroxId, GameObject> nonSplineEntitiesById = NitroxEntity.GetObjectsFrom(watchingEntityIds)
                                                                                     .Where(item => !item.Value.GetComponent<SwimBehaviour>() && 
                                                                                                    !item.Value.GetComponent<WalkBehaviour>())
                                                                                     .ToDictionary(item => item.Key, item => item.Value);
                
                List<EntityTransformUpdate> updates = BuildUpdates(nonSplineEntitiesById);

                if (updates.Count > 0)
                {
                    packetSender.Send(new EntityTransformUpdates(updates));
                }
            }
        }
    }

    private List<EntityTransformUpdate> BuildUpdates(Dictionary<NitroxId, GameObject> nonSplineEntitiesById)
    {
        List<EntityTransformUpdate> updates = new();

        foreach (KeyValuePair<NitroxId, GameObject> gameObjectWithId in nonSplineEntitiesById)
        {
            if (gameObjectWithId.Value)
            {
                if (ShouldBroadcastLocalPosition(gameObjectWithId.Value))
                {
                    updates.Add(new RawTransformUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.localPosition.ToDto(), gameObjectWithId.Value.transform.localRotation.ToDto()));
                }
                else
                {
                    updates.Add(new RawTransformUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.position.ToDto(), gameObjectWithId.Value.transform.rotation.ToDto()));
                }
            }
        }

        // Only send data for entities still simulated by the local player
        updates.AddRange(splineUpdatesById.Values.Where(
            splineUpdate => this.Resolve<SimulationOwnership>().HasAnyLockType(splineUpdate.Id)
        ));
        
        splineUpdatesById.Clear();

        return updates;
    }

    public static void WatchEntity(NitroxId id)
    {
        watchingEntityIds.Add(id);

        // The game object may not exist at this very moment (due to being spawned in async). This is OK as we will
        // automatically start sending updates when we finally get it in the world. This behavior will also allow us
        // to resync or respawn entities while still have broadcasting enabled without doing anything extra.
        
        if (NitroxEntity.TryGetComponentFrom(id, out RemotelyControlled remotelyControlled))
        {
            Object.Destroy(remotelyControlled);
        }
    }

    public static void StopWatchingEntity(NitroxId id)
    {
        watchingEntityIds.Remove(id);
    }

    public static void RegisterSplineMovementChange(NitroxId id, GameObject gameObject, Vector3 targetPos, Vector3 targetDir, float velocity)
    {
        if (watchingEntityIds.Contains(id))
        {
            if (ShouldBroadcastLocalPosition(gameObject))
            {
                splineUpdatesById[id] = new(id, gameObject.transform.localPosition.ToDto(), gameObject.transform.localRotation.ToDto(), targetPos.ToDto(), targetDir.ToDto(), velocity);
            }
            else
            {
                splineUpdatesById[id] = new(id, gameObject.transform.position.ToDto(), gameObject.transform.rotation.ToDto(), targetPos.ToDto(), targetDir.ToDto(), velocity);
            }
        }
    }

    public static void RemoveEntityMovementControl(GameObject gameObject, NitroxId entityId)
    {
        if (gameObject.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            Destroy(remotelyControlled);
        }
        StopWatchingEntity(entityId);
    }

    private static bool ShouldBroadcastLocalPosition(GameObject gameObject)
    {
        // Fishes in WaterParks are located under WaterPark/items_root
        Transform parent = gameObject.transform.parent;
        return parent && parent.name.Equals("items_root");
    }
}

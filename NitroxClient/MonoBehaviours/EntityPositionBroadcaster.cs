using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace NitroxClient.MonoBehaviours;

public class EntityPositionBroadcaster : MonoBehaviour
{
    public static EntityPositionBroadcaster Instance;

    public static readonly float BROADCAST_INTERVAL = 0.25f;

    /// <summary>
    /// Dictionary of watched entities that don't follow spline movements.
    /// </summary>
    private readonly Dictionary<NitroxId, GameObject> regularEntities = [];
    /// <summary>
    /// Dictionary of watched entities that follow spline movements.
    /// </summary>
    private readonly Dictionary<NitroxId, SwimBehaviour> splineEntities = [];
    /// <summary>
    /// Set of watched entities that weren't spawned yet.
    /// </summary>
    private readonly HashSet<NitroxId> notSpawnedEntityIds = [];
    /// <summary>
    /// Latest registered spline updates from SplineFollowing.GoTo
    /// </summary>
    private readonly Dictionary<NitroxId, SplineTransformUpdate> splineUpdatesById = [];
    /// <summary>
    /// Reusable list of <see cref="EntityTransformUpdate"/>s to avoid reallocating a new list at each broadcast.
    /// </summary>
    /// <remarks>
    /// This only works because <see cref="LiteNetLibClient.Send"/> immediately serialiazes the list.
    /// </remarks>
    private readonly List<EntityTransformUpdate> updates = new(50);

    private IPacketSender packetSender;
    private SimulationOwnership simulationOwnership;

    private float time;

    public void Awake()
    {
        if (Instance)
        {
            Log.Error($"There's already a {nameof(EntityPositionBroadcaster)} Instance alive, destroying the new one.");
            Destroy(this);
            return;
        }
        Instance = this;

        packetSender = this.Resolve<IPacketSender>();
        simulationOwnership = this.Resolve<SimulationOwnership>();
    }

    public void Update()
    {
        time += Time.deltaTime;

        // Only do on a specific cadence to avoid hammering server
        if (time >= BROADCAST_INTERVAL)
        {
            time = 0;

            CheckEntities();
            BuildUpdates();

            if (updates.Count > 0)
            {
                packetSender.Send(new EntityTransformUpdates(updates));
            }
        }
    }

    private void BuildUpdates()
    {
        // Avoid any GC allocation
        updates.Clear();

        foreach (KeyValuePair<NitroxId, GameObject> entityPair in regularEntities)
        {
            Transform entityTransform = entityPair.Value.transform;
            updates.Add(new RawTransformUpdate(entityPair.Key, entityTransform.position.ToDto(), entityTransform.rotation.ToDto()));
        }

        // Only send data for entities still simulated by the local player
        foreach (SplineTransformUpdate splineUpdate in splineUpdatesById.Values)
        {
            if (simulationOwnership.HasAnyLockType(splineUpdate.Id))
            {
                updates.Add(splineUpdate);
            }
        }
        
        splineUpdatesById.Clear();
    }

    public void WatchEntity(NitroxId id)
    {
        // The game object may not exist at this very moment (due to being spawned in async). This is OK as we will
        // automatically start sending updates when we finally get it in the world. This behavior will also allow us
        // to resync or respawn entities while still have broadcasting enabled without doing anything extra.
     
        if (NitroxEntity.TryGetObjectFrom(id, out GameObject entityObject))
        {
            SortEntity(id, entityObject);
        }
        else
        {
            notSpawnedEntityIds.Add(id);
        }
    }

    private void SortEntity(NitroxId nitroxId, GameObject entityObject)
    {
        if (entityObject.TryGetComponent(out SwimBehaviour swimBehaviour) && swimBehaviour.enabled)
        {
            splineEntities[nitroxId] = swimBehaviour;
        }
        else
        {
            regularEntities[nitroxId] = entityObject;
        }

        if (entityObject.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            Object.Destroy(remotelyControlled);
        }
    }

    private void CheckEntities()
    {
        // when fishes die, they're only a corpse and their swim behaviour stops functioning
        splineEntities.RemoveWhere(pair =>
        {
            SwimBehaviour swimBehaviour = pair.Value;

            if (!swimBehaviour)
            {
                notSpawnedEntityIds.Add(pair.Key);
                return true;
            }

            if (!swimBehaviour.enabled)
            {
                regularEntities[pair.Key] = swimBehaviour.gameObject;
                return true;
            }
            return false;
        });

        regularEntities.RemoveWhere(pair =>
        {
            if (!pair.Value)
            {
                notSpawnedEntityIds.Add(pair.Key);
                return true;
            }
            return false;
        });

        // in case a fish was removed from splineEntities (from the above loop), it can be added back in here as a regular entity if required
        // NB: keep this section below the other RemoveWhere sections so it can eventually collect fresh references from the NitroxIds
        notSpawnedEntityIds.RemoveWhere(id =>
        {
            if (NitroxEntity.TryGetObjectFrom(id, out GameObject entityObject))
            {
                SortEntity(id, entityObject);
                return true;
            }
            return false;
        });
    }

    public void StopWatchingEntity(NitroxId id)
    {
        splineEntities.Remove(id);
        regularEntities.Remove(id);
        notSpawnedEntityIds.Remove(id);
    }

    public void RegisterSplineMovementChange(NitroxId id, GameObject gameObject, Vector3 targetPos, Vector3 targetDir, float velocity)
    {
        if (splineEntities.ContainsKey(id))
        {
            splineUpdatesById[id] = new(id, gameObject.transform.position.ToDto(), gameObject.transform.rotation.ToDto(), targetPos.ToDto(), targetDir.ToDto(), velocity);
        }
    }

    public void RemoveEntityMovementControl(GameObject gameObject, NitroxId entityId)
    {
        if (gameObject.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            Destroy(remotelyControlled);
        }
        StopWatchingEntity(entityId);
    }
}

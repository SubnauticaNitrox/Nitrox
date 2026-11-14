using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.GameLogic;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace NitroxClient.MonoBehaviours;

public sealed class EntityPositionBroadcaster : MonoBehaviour
{
    public static EntityPositionBroadcaster Instance;

    /// <summary>
    ///     The time between two broadcasts in seconds.
    /// </summary>
    public static readonly float BROADCAST_INTERVAL = 0.1f;

    /// <summary>
    ///     Set of watched entities that weren't spawned yet.
    /// </summary>
    private readonly HashSet<NitroxId> notSpawnedEntityIds = [];

    /// <summary>
    ///     Dictionary of watched entities that don't follow spline movements.
    /// </summary>
    private readonly Dictionary<NitroxId, GameObject> regularEntities = [];

    /// <summary>
    ///     Dictionary of watched entities that follow spline movements.
    /// </summary>
    private readonly Dictionary<NitroxId, SwimBehaviour> splineEntities = [];

    /// <summary>
    ///     Latest registered spline updates from SplineFollowing.GoTo
    /// </summary>
    private readonly Dictionary<NitroxId, SplineTransformUpdate> splineUpdatesById = [];

    /// <summary>
    ///     Reusable list of <see cref="EntityTransformUpdate" />s to avoid reallocating a new list at each broadcast.
    /// </summary>
    /// <remarks>
    ///     This only works because <see cref="LiteNetLibClient.Send" /> immediately serializes the list.
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

            ReassignEntitiesToLookups();
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
            if (simulationOwnership.HasAnyLockType(splineUpdate.Id) && splineEntities.TryGetValue(splineUpdate.Id, out SwimBehaviour swimBehaviour))
            {
                Transform entityTransform = swimBehaviour.transform;
                splineUpdate.Position = entityTransform.position.ToDto();
                splineUpdate.Rotation = entityTransform.rotation.ToDto();
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
            AddEntityToLookup(id, entityObject);
        }
        else
        {
            notSpawnedEntityIds.Add(id);
        }
    }

    private void AddEntityToLookup(NitroxId nitroxId, GameObject entityObject)
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
            Destroy(remotelyControlled);
        }
    }

    /// <summary>
    ///     For each tracked entity, ensures it stays in the right HashSet/Dictionary depending on its state.
    ///     Either the entity has not spawned (<see cref="notSpawnedEntityIds" />) or it follows a spline (
    ///     <see cref="splineEntities" />)
    ///     or in the default case (<see cref="regularEntities" />).
    /// </summary>
    private void ReassignEntitiesToLookups()
    {
        // when fishes die, they're only a corpse and their swim behaviour stops functioning
        splineEntities.RemoveWhere(this, static (self, pair) =>
        {
            SwimBehaviour swimBehaviour = pair.Value;
            if (!swimBehaviour)
            {
                self.notSpawnedEntityIds.Add(pair.Key);
                return true;
            }
            if (!swimBehaviour.enabled)
            {
                self.regularEntities[pair.Key] = swimBehaviour.gameObject;
                return true;
            }
            return false;
        });

        regularEntities.RemoveWhere(this, static (self, pair) =>
        {
            if (!pair.Value)
            {
                self.notSpawnedEntityIds.Add(pair.Key);
                return true;
            }
            return false;
        });

        // in case a fish was removed from splineEntities (from the above loop), it can be added back in here as a regular entity if required
        // NB: keep this section below the other RemoveWhere sections so it can eventually collect fresh references from the NitroxIds
        notSpawnedEntityIds.RemoveWhere(this, static (self, id) =>
        {
            if (NitroxEntity.TryGetObjectFrom(id, out GameObject entityObject))
            {
                self.AddEntityToLookup(id, entityObject);
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

    public void ClearNotSpawnedEntities()
    {
        notSpawnedEntityIds.Clear();
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

    /// <summary>
    ///     Notifies the server of the latest known position of this entity if the local player is simulating it.
    /// </summary>
    public void SendLastUpdateAndDropOwnership(GameObject gameObject)
    {
        if (!gameObject.TryGetNitroxId(out NitroxId entityId) || !simulationOwnership.HasAnyLockType(entityId))
        {
            return;
        }

        Transform entityTransform = gameObject.transform;
        EntityTransformUpdate entityTransformUpdate = null;

        if (splineEntities.ContainsKey(entityId) && gameObject.TryGetComponent(out SplineFollowing splineFollowing))
        {
            // Clean up in case there remains an update that wasn't sent yet
            splineUpdatesById.Remove(entityId);
            entityTransformUpdate = new SplineTransformUpdate(entityId, entityTransform.position.ToDto(), entityTransform.rotation.ToDto(), splineFollowing.targetPosition.ToDto(), splineFollowing.targetDirection.ToDto(), splineFollowing.medianSpeed);
        }
        else if (regularEntities.ContainsKey(entityId))
        {
            entityTransformUpdate = new RawTransformUpdate(entityId, entityTransform.position.ToDto(), entityTransform.rotation.ToDto());
        }

        if (entityTransformUpdate != null)
        {
            packetSender.Send(new LastEntityTransformUpdate(entityTransformUpdate));
        }

        // locally drop simulation
        simulationOwnership.DropSimulationFrom(entityId);
    }
}

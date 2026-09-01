using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Core;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace NitroxClient.Services.Multiplayer;

internal sealed class EntityPositionBroadcastService(IPacketSender packetSender, SimulationOwnership simulationOwnership) : IMultiplayerGameService
{
    public static readonly float BROADCAST_INTERVAL = 0.25f;

    private readonly HashSet<NitroxId> watchingEntityIds = [];
    private readonly Dictionary<NitroxId, SplineTransformUpdate> splineUpdatesById = [];
    private readonly IPacketSender packetSender = packetSender;
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;

    private float time;

    public void StopWatchingEntity(NitroxId id)
    {
        watchingEntityIds.Remove(id);
    }

    public void RegisterSplineMovementChange(NitroxId id, GameObject gameObject, Vector3 targetPos, Vector3 targetDir, float velocity)
    {
        if (watchingEntityIds.Contains(id))
        {
            splineUpdatesById[id] = new(id, gameObject.transform.position.ToDto(), gameObject.transform.rotation.ToDto(), targetPos.ToDto(), targetDir.ToDto(), velocity);
        }
    }

    public void RemoveEntityMovementControl(GameObject gameObject, NitroxId entityId)
    {
        if (gameObject.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            Object.Destroy(remotelyControlled);
        }
        StopWatchingEntity(entityId);
    }

    public void WatchEntity(NitroxId id)
    {
        watchingEntityIds.Add(id);

        // The game object may not exist at this very moment (due to being spawned in async). This is OK as we will
        // automatically start sending updates when we finally get it in the world. This behavior will also allow us
        // to resync or respawn entities while still have broadcasting enabled without doing anything extra.

        if (NitroxEntity.TryGetComponentFrom(id, out RemotelyControlled remotelyControlled))
        {
            Destroy(remotelyControlled);
        }
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

    public void Start()
    {
    }

    public void Started()
    {
    }

    public void Stop()
    {
    }

    private List<EntityTransformUpdate> BuildUpdates(Dictionary<NitroxId, GameObject> nonSplineEntitiesById)
    {
        List<EntityTransformUpdate> updates = new();

        foreach (KeyValuePair<NitroxId, GameObject> gameObjectWithId in nonSplineEntitiesById)
        {
            if (gameObjectWithId.Value)
            {
                updates.Add(new RawTransformUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.position.ToDto(), gameObjectWithId.Value.transform.rotation.ToDto()));
            }
        }

        // Only send data for entities still simulated by the local player
        updates.AddRange(splineUpdatesById.Values.Where(splineUpdate => simulationOwnership.HasAnyLockType(splineUpdate.Id)));

        splineUpdatesById.Clear();

        return updates;
    }
}

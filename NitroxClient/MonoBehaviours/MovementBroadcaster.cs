using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementBroadcaster : MonoBehaviour
{
    public const int BROADCAST_FREQUENCY = 30;
    public const float BROADCAST_PERIOD = 1f / BROADCAST_FREQUENCY;

    private readonly Dictionary<NitroxId, WatchedEntry> watchedEntries = [];
    public Dictionary<NitroxId, MovementReplicator> Replicators = [];
    private float latestBroadcastTime;
    public static MovementBroadcaster Instance;

    public void Start()
    {
        if (Instance)
        {
            Log.Error($"There's already a {nameof(MovementBroadcaster)} Instance alive, destroying the new one.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void Update()
    {
        float currentTime = DayNightCycle.main.timePassedAsFloat;
        if (currentTime < latestBroadcastTime + BROADCAST_PERIOD)
        {
            return;
        }
        latestBroadcastTime = currentTime;
        BroadcastLocalData(currentTime);
    }

    public void BroadcastLocalData(float time)
    {
        Dictionary<NitroxId, MovementData> data = [];
        foreach (KeyValuePair<NitroxId, WatchedEntry> entry in watchedEntries)
        {
            // TODO: Don't broadcast at certain times: while docking, while docked ...
            data.Add(entry.Key, entry.Value.GetMovementData());
        }

        if (data.Count > 0)
        {
            this.Resolve<IPacketSender>().Send(new VehicleMovements(data, time));
        }
    }

    public static void RegisterWatched(GameObject gameObject, NitroxId entityId)
    {
        if (!Instance)
        {
            return;
        }

        if (!Instance.watchedEntries.ContainsKey(entityId))
        {
            Instance.watchedEntries.Add(entityId, new(gameObject));
        }
    }

    public static void UnregisterWatched(NitroxId entityId)
    {
        if (Instance)
        {
            Instance.watchedEntries.Remove(entityId);
        }
    }

    public static void RegisterReplicator(MovementReplicator movementReplicator)
    {
        if (Instance)
        {
            Instance.Replicators.Add(movementReplicator.objectId, movementReplicator);
        }
    }

    public static void UnregisterReplicator(MovementReplicator movementReplicator)
    {
        if (Instance)
        {
            Instance.Replicators.Remove(movementReplicator.objectId);
        }
    }

    private record struct WatchedEntry
    {
        private Vehicle vehicle;
        private SubRoot subRoot;
        private Rigidbody rigidbody;

        public WatchedEntry(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out vehicle))
            {
                rigidbody = vehicle.GetComponent<Rigidbody>();
            }
            else if (gameObject.TryGetComponent(out SubRoot subRoot))
            {
                rigidbody = subRoot.GetComponent<Rigidbody>();
            }
        }

        public MovementData GetMovementData()
        {
            return new(rigidbody.position.ToDto(), rigidbody.velocity.ToDto(), rigidbody.rotation.ToDto(), rigidbody.angularVelocity.ToDto());
        }
    }    
}

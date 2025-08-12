using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementBroadcaster : MonoBehaviour
{
    public const int BROADCAST_FREQUENCY = 30;
    public const float BROADCAST_PERIOD = 1f / BROADCAST_FREQUENCY;

    public static MovementBroadcaster Instance;

    public Dictionary<NitroxId, MovementReplicator> Replicators = [];
    private readonly Dictionary<NitroxId, WatchedEntry> watchedEntries = [];
    private float latestBroadcastTime;

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
        float currentTime = (float)this.Resolve<TimeManager>().RealTimeElapsed;
        if (currentTime < latestBroadcastTime + BROADCAST_PERIOD)
        {
            return;
        }
        latestBroadcastTime = currentTime;
        BroadcastLocalData(currentTime);
    }

    public void BroadcastLocalData(float time)
    {
        List<MovementData> data = [];
        List<NitroxId> watchedIds = [..watchedEntries.Keys];

        for (int i = watchedIds.Count - 1; i >= 0; i--)
        {
            NitroxId entryId = watchedIds[i];
            WatchedEntry entry = watchedEntries[entryId];
            
            if (entry.ShouldBroadcastMovement())
            {
                data.Add(entry.GetMovementData(entryId));
                entry.OnBroadcastPosition();
            }
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
            Instance.watchedEntries.Add(entityId, new(entityId, gameObject.transform));
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
}

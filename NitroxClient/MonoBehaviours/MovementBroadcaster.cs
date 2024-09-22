using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementBroadcaster : MonoBehaviour
{
    public const int FIXED_TICK_RATE_MS = 50;
    public const float TIME_PER_TICK = 1 / FIXED_TICK_RATE_MS;
    private float latestUpdateTime;
    public static int Tick;

    private readonly Dictionary<NitroxId, WatchedEntry> watchedEntries = [];
    public Dictionary<NitroxId, MovementReplicator> Replicators = [];
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
        StartCoroutine(BroadcastLoop());
        //GameLoop().ContinueWithHandleError(Log.Error);
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    // TODO: actually no need for ticks: https://www.youtube.com/watch?v=YR6Bc0-6YJA

    private double TickDouble => this.Resolve<TimeManager>().RealTimeElapsed / (FIXED_TICK_RATE_MS * 0.001d);
    private int CurrentTick => (int)Math.Floor(TickDouble);

    private int TimeBeforeNextTick()
    {
        // ex: tickDouble = 3487.2
        // tickDouble -   Mathf.Floor(tickDouble) = 0.2
        // 1          -   0.2                     = 0.8
        double tickDouble = TickDouble;
        return (int)Math.Round(1000 * (1 - (tickDouble - Math.Floor(tickDouble))) / FIXED_TICK_RATE_MS);
    }

    public async Task GameLoop()
    {
        while (true)
        {
            await Task.Delay(TimeBeforeNextTick());
            Tick++;
        }
    }

    // TODO: if this eventually works, deprecate RemotelyControlled and move everything to this system
    public IEnumerator BroadcastLoop()
    {
        while (true)
        {
            float time = (float)this.Resolve<TimeManager>().RealTimeElapsed;
            float deltaTime = time - latestUpdateTime;

            // Happens during loading and freezes
            if (deltaTime == 0)
            {
                yield return null;
                continue;
            }

            if (deltaTime < TIME_PER_TICK)
            {
                yield return new WaitForSeconds((float)deltaTime);
            }
            latestUpdateTime = time;

            BroadcastLocalData(time);
            ProcessReplicators(time, deltaTime);
        }
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

    public void ProcessReplicators(float time, float deltaTime)
    {
        foreach (MovementReplicator movementReplicator in Replicators.Values)
        {
            //movementReplicator.ReplicatorFixedUpdate(time, deltaTime);
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

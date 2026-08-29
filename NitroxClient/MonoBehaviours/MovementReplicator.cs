using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxClient.MonoBehaviours.Vehicles;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public abstract class MovementReplicator : MonoBehaviour
{
    public const float INTERPOLATION_TIME = 4 * MovementBroadcaster.BROADCAST_PERIOD;
    public const float SNAPSHOT_EXPIRATION_TIME = 5f * INTERPOLATION_TIME;

    private TimeManager timeManager;

    private readonly RingBuffer<Snapshot> buffer = new();
    /// <summary>
    /// To ensure a smooth experience, we need a max allowed latency value which should top the incoming latencies at all times.
    /// Big increments and any decrements of this value will likely cause stutter, so we try to avoid changing this value too much.
    /// But it is required that after a lag spike, we eventually lower down that value, which is done periodically <see cref="NitroxPrefs.LatencyUpdatePeriod"/>.
    /// </summary>
    public float MaxAllowedLatency;

    private float latestLatencyBumpTime;
    private float maxLatencyDetectedRecently;

    /// <summary>
    /// When encountering a latency bump, we must expect worse happening right after, so we add this margin to our new <see cref="MaxAllowedLatency"/>.
    /// After each periodical latency update (<see cref="LatencyUpdatePeriod"/>), we only want to lower the latency if it's way smaller than the current variable latency.
    /// The safety threshold is defined by this value.
    /// </summary>
    private float SafetyLatencyMargin => NitroxPrefs.SafetyLatencyMargin.Value;

    private float LatencyUpdatePeriod => NitroxPrefs.LatencyUpdatePeriod.Value;

    private Rigidbody rigidbody;
    public NitroxId objectId { get; private set; }

    /// <summary>
    /// Used time must be based on real time to avoid effects from time changes/speed.
    /// </summary>
    private float RealTime => (float)timeManager.RealTimeElapsed;

    public void Awake()
    {
        timeManager = this.Resolve<TimeManager>();
    }

    public void AddSnapshot(MovementData movementData, float snapshotRealTime)
    {
        float realTime = RealTime;
        float latency = realTime - snapshotRealTime;


        RecalculateMaxAllowedLatency(latency, realTime);

        float occurrenceTime = snapshotRealTime + INTERPOLATION_TIME + MaxAllowedLatency;

        // Cleaning any previous value change that would occur later than the newly received snapshot
        while (buffer.Count > 0)
        {
            if (buffer.Last.IsSnapshotNewer(occurrenceTime))
            {
                buffer.RemoveLast();
            }
            else
            {
                break;
            }
        }

        buffer.Add(new Snapshot(movementData, occurrenceTime));
    }

    private void RecalculateMaxAllowedLatency(float latency, float realTime)
    {
        if (latency > MaxAllowedLatency)
        {
            MaxAllowedLatency = latency + SafetyLatencyMargin;
            latestLatencyBumpTime = realTime;
            maxLatencyDetectedRecently = 0;
            return;
        }

        maxLatencyDetectedRecently = Mathf.Max(latency, maxLatencyDetectedRecently);

        if (realTime - latestLatencyBumpTime < LatencyUpdatePeriod)
        {
            return;
        }

        if (maxLatencyDetectedRecently < MaxAllowedLatency - 2 * SafetyLatencyMargin)
        {
            MaxAllowedLatency = maxLatencyDetectedRecently + SafetyLatencyMargin; // regular gameplay latency variation
        }
        latestLatencyBumpTime = realTime;
        maxLatencyDetectedRecently = 0;
    }

    public void ClearBuffer() => buffer.Clear();

    public void Start()
    {
        if (!gameObject.TryGetNitroxId(out NitroxId _objectId))
        {
            Log.Error($"Can't start a {nameof(MovementReplicator)} on {name} because it doesn't have an attached: {nameof(NitroxEntity)}");
            Destroy(this);
            return;
        }
        objectId = _objectId;

        rigidbody = GetComponent<Rigidbody>();
        if (gameObject.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.SetReceiving();
        }
        else
        {
            if (gameObject.TryGetComponent(out WorldForces worldForces))
            {
                worldForces.enabled = false;
            }
            rigidbody.isKinematic = false;
        }

        MovementBroadcaster.RegisterReplicator(this);
    }

    public void OnDestroy()
    {
        if (gameObject.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.SetBroadcasting();
        }
        else
        {
            if (gameObject.TryGetComponent(out WorldForces worldForces))
            {
                worldForces.enabled = true;
            }
        }

        MovementBroadcaster.UnregisterReplicator(this);
    }

    public void Update()
    {
        if (buffer.Count == 0)
        {
            return;
        }

        float realTime = RealTime;

        // Sorting out expired nodes
        while (buffer.Count > 0 && buffer.First.IsExpired(realTime))
        {
            buffer.RemoveFirst();
        }

        // No usable nodes left
        if (buffer.Count == 0)
        {
            return;
        }

        // Current node is not useable yet
        if (buffer.First.IsSnapshotNewer(realTime))
        {
            return;
        }

        // Purging the next nodes if they should have already happened
        while (buffer.Count > 1)
        {
            if (!buffer[buffer.Head + 1].IsSnapshotNewer(realTime))
            {
                buffer.RemoveFirst();
            }
            else
            {
                break;
            }
        }

        // Need at least two snapshots to interpolate
        if (buffer.Count < 2)
        {
            return;
        }

        // Interpolation
        Snapshot previousSnapshot = buffer.First;
        Snapshot nextSnapshot = buffer[buffer.Head + 1];

        float t = (realTime - previousSnapshot.Time) / (nextSnapshot.Time - previousSnapshot.Time);

        transform.position = Vector3.Lerp(previousSnapshot.Data.Position.ToUnity(), nextSnapshot.Data.Position.ToUnity(), t);
        
        transform.rotation = Quaternion.Lerp(previousSnapshot.Data.Rotation.ToUnity(), nextSnapshot.Data.Rotation.ToUnity(), t);

        ApplyNewMovementData(nextSnapshot.Data);

        // TODO: fix remote players being able to go through the object (ex: cyclops)
    }

    public abstract void ApplyNewMovementData(MovementData newMovementData);

    public record struct Snapshot(MovementData Data, float Time)
    {
        public bool IsSnapshotNewer(float comparedTime) => comparedTime < Time;

        public bool IsExpired(float comparedTime) => comparedTime > Time + SNAPSHOT_EXPIRATION_TIME;
    }

    public static MovementReplicator AddReplicatorToObject(GameObject gameObject)
    {
        if (gameObject.GetComponent<SeaMoth>())
        {
            return gameObject.AddComponent<SeamothMovementReplicator>();
        }
        if (gameObject.GetComponent<Exosuit>())
        {
            return gameObject.AddComponent<ExosuitMovementReplicator>();
        }
        if (gameObject.GetComponent<SubControl>())
        {
            return gameObject.AddComponent<CyclopsMovementReplicator>();
        }
        return gameObject.AddComponent<MovementReplicator>();
    }
}

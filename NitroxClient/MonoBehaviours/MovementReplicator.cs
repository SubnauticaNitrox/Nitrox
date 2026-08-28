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
    /// Current time must be based on real time to avoid effects from time changes/speed.
    /// </summary>
    private float CurrentTime => (float)this.Resolve<TimeManager>().RealTimeElapsed;

    public void AddSnapshot(MovementData movementData, float time)
    {
        float currentTime = CurrentTime;
        float latency = currentTime - time;

        if (latency > MaxAllowedLatency)
        {
            MaxAllowedLatency = latency + SafetyLatencyMargin;
            latestLatencyBumpTime = currentTime;
            maxLatencyDetectedRecently = 0;
        }
        else
        {
            maxLatencyDetectedRecently = Mathf.Max(latency, maxLatencyDetectedRecently);

            if (currentTime - latestLatencyBumpTime >= LatencyUpdatePeriod)
            {
                if (maxLatencyDetectedRecently < MaxAllowedLatency - 2 * SafetyLatencyMargin)
                {
                    MaxAllowedLatency = maxLatencyDetectedRecently + SafetyLatencyMargin; // regular gameplay latency variation
                }
                latestLatencyBumpTime = currentTime;
                maxLatencyDetectedRecently = 0;
            }
        }

        float occurrenceTime = time + INTERPOLATION_TIME + MaxAllowedLatency;

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

        float currentTime = CurrentTime;

        // Sorting out expired nodes
        while (buffer.Count > 0 && buffer.First.IsExpired(currentTime))
        {
            buffer.RemoveFirst();
        }

        // No usable nodes left
        if (buffer.Count == 0)
        {
            return;
        }

        // Current node is not useable yet
        if (buffer.First.IsSnapshotNewer(currentTime))
        {
            return;
        }

        // Purging the next nodes if they should have already happened
        while (buffer.Count > 1)
        {
            if (!buffer[buffer.Head + 1].IsSnapshotNewer(currentTime))
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

        float t = (currentTime - previousSnapshot.Time) / (nextSnapshot.Time - previousSnapshot.Time);

        transform.position = Vector3.Lerp(previousSnapshot.Data.Position.ToUnity(), nextSnapshot.Data.Position.ToUnity(), t);
        
        transform.rotation = Quaternion.Lerp(previousSnapshot.Data.Rotation.ToUnity(), nextSnapshot.Data.Rotation.ToUnity(), t);

        ApplyNewMovementData(nextSnapshot.Data);

        // TODO: fix remote players being able to go through the object (ex: cyclops)
    }

    public abstract void ApplyNewMovementData(MovementData newMovementData);

    public record struct Snapshot(MovementData Data, float Time)
    {
        public bool IsSnapshotNewer(float currentTime) => currentTime < Time;

        public bool IsExpired(float currentTime) => currentTime > Time + SNAPSHOT_EXPIRATION_TIME;
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

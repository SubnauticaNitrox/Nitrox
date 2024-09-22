using System.Collections.Generic;
using System.Linq;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementReplicator : MonoBehaviour
{
    public const float MAX_POSITION_ERROR = 10;
    public const float MAX_ROTATION_ERROR = 20; // °
    public const float INTERPOLATION_TIME = 4 * MovementBroadcaster.TIME_PER_TICK;
    public const float SNAPSHOT_EXPIRATION_TIME = 5f * INTERPOLATION_TIME;
    private const int BUFFER_SIZE = 10;

    private readonly CircularBuffer<Snapshot> bufferedSnapshots = new(BUFFER_SIZE);

    public Rigidbody rigidbody;
    public NitroxId objectId;

    public void AddSnapshot(MovementData movementData, float time)
    {
        bufferedSnapshots.Add(new(movementData, time));
    }

    // TODO: add interpolation time (probably like 2 frames)

    public void Start()
    {
        if (!gameObject.TryGetNitroxId(out objectId))
        {
            Log.Error($"Can't start a {nameof(MovementReplicator)} on {name} because it doesn't have an attached: {nameof(NitroxEntity)}");
            return;
        }

        if (gameObject.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.SetReceiving();
        }
        else if (gameObject.TryGetComponent(out WorldForces worldForces))
        {
            worldForces.enabled = false;
        }
        rigidbody = GetComponent<Rigidbody>();
        
        MovementBroadcaster.RegisterReplicator(this);
    }

    public void OnDestroy()
    {
        if (gameObject.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.SetBroadcasting();
        }
        else if (gameObject.TryGetComponent(out WorldForces worldForces))
        {
            worldForces.enabled = false;
        }

        MovementBroadcaster.UnregisterReplicator(this);
    }

    public void ReplicatorFixedUpdate(float time, float deltaTime)
    {
        if (bufferedSnapshots.Count == 0)
        {
            return;
        }

        float renderTime = time - INTERPOLATION_TIME;

        bool isSnapshotExpired(Snapshot snapshot)
        {
            return snapshot.Time + SNAPSHOT_EXPIRATION_TIME < renderTime;
        }

        List<Snapshot> orderedSnapshots = [.. bufferedSnapshots.OrderBy(s => s.Time)];
        if (orderedSnapshots.Count == 0)
        {
            return;
        }
        if (orderedSnapshots.Count == 1)
        {
            // We just wait for another snapshot if the only one we got is not treatable yet
            if (renderTime < orderedSnapshots[0].Time)
            {
                return;
            }

            // If we've gone past our only snapshot, we'll extrapolate unless the snapshot has expired
            // in which case extrapolation might not be relevant
            if (isSnapshotExpired(orderedSnapshots[0]))
            {
                bufferedSnapshots.RemoveAt(0);
                return;
            }
            Extrapolate(orderedSnapshots[0], time, deltaTime);
        }
        else // At least 2 valid snapshots
        {
            Snapshot firstBefore = default;
            Snapshot firstAfter = default;
            foreach (Snapshot snapshot in orderedSnapshots)
            {
                if (firstBefore == default && renderTime >= snapshot.Time)
                {
                    firstBefore = snapshot;
                }
                else if (firstAfter == default && renderTime < snapshot.Time)
                {
                    firstAfter = snapshot;
                    break;
                }
            }
            if (firstBefore == default)
            {

                // Do something
                return;
            }
            if (firstAfter == default)
            {
                // Do something
                return;
            }
            Interpolate(firstBefore, firstAfter, time);
        }
    }

    private void Interpolate(Snapshot prevSnapshot, Snapshot nextSnapshot, float time)
    {
        float deltaTime = nextSnapshot.Time - prevSnapshot.Time;
        float t = (time - prevSnapshot.Time) / deltaTime;

        rigidbody.position = Vector3.Lerp(prevSnapshot.Data.Position.ToUnity(), nextSnapshot.Data.Position.ToUnity(), t);
    }

    private void Extrapolate(Snapshot snapshot, float time, float deltaTime)
    {
        float movementDeltaTime = (float)(time - snapshot.Time);
        Vector3 estimatedPosition = snapshot.Data.Position.ToUnity() + movementDeltaTime * snapshot.Data.Velocity.ToUnity();

        Vector3 positionError = estimatedPosition - rigidbody.position;

        if (positionError.magnitude > MAX_POSITION_ERROR)
        {
            rigidbody.position = Vector3.Lerp(rigidbody.position, estimatedPosition, (float)deltaTime * 10);
            Log.InGame($"MAX POS ERR: {positionError.magnitude}");
        }

        Quaternion estimatedRotation = snapshot.Data.Rotation.ToUnity() * Quaternion.Euler(snapshot.Data.AngularVelocity.ToUnity() * movementDeltaTime);

        float rotationError = Quaternion.Angle(estimatedRotation, rigidbody.rotation);
        if (rotationError > MAX_ROTATION_ERROR)
        {
            rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, estimatedRotation, (float)deltaTime * 10);
            Log.InGame($"MAX ROT ERR: {rotationError}");
        }

        rigidbody.velocity = snapshot.Data.Velocity.ToUnity();
        rigidbody.angularVelocity = snapshot.Data.AngularVelocity.ToUnity();
    }

    private record struct Snapshot(MovementData Data, float Time);
}

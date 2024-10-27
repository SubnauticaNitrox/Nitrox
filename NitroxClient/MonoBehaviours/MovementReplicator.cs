using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MovementReplicator : MonoBehaviour
{
    public const float INTERPOLATION_TIME = 4 * MovementBroadcaster.BROADCAST_PERIOD;
    public const float SNAPSHOT_EXPIRATION_TIME = 5f * INTERPOLATION_TIME;
    private const int BUFFER_SIZE = 10;

    private readonly LinkedList<Snapshot> buffer = new();

    public Rigidbody rigidbody;
    public NitroxId objectId;

    public void AddSnapshot(MovementData movementData, float time)
    {
        buffer.AddLast(new Snapshot(movementData, time + INTERPOLATION_TIME + NitroxPrefs.MovementLatency.Value));
    }

    public void Start()
    {
        if (!gameObject.TryGetNitroxId(out objectId))
        {
            Log.Error($"Can't start a {nameof(MovementReplicator)} on {name} because it doesn't have an attached: {nameof(NitroxEntity)}");
            return;
        }

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
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
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
            rigidbody.isKinematic = false;
            rigidbody.interpolation = RigidbodyInterpolation.None;
        }

        MovementBroadcaster.UnregisterReplicator(this);
    }

    public void Update()
    {
        if (buffer.Count == 0)
        {
            return;
        }

        float currentTime = DayNightCycle.main.timePassedAsFloat;

        // Sorting out expired nodes
        while (buffer.First != null && buffer.First.Value.IsExpired(currentTime))
        {
            // Log.Debug($"Invalid node: {currentTime} > {buffer.First.Value.Time + SNAPSHOT_EXPIRATION_TIME}");
            buffer.RemoveFirst();
        }

        LinkedListNode<Snapshot> firstNode = buffer.First;
        if (firstNode == null)
        {
            // Log.Debug("nothing next");
            return;
        }

        // Current node is not useable yet
        if (firstNode.Value.IsOlderThan(currentTime))
        {
            // Log.Debug($"too early {currentTime} < {firstNode.Value.Time}");
            return;
        }

        while (firstNode.Next != null && !firstNode.Next.Value.IsOlderThan(currentTime))
        {
            firstNode = firstNode.Next;
            buffer.RemoveFirst();
        }

        LinkedListNode<Snapshot> nextNode = firstNode.Next;

        // No next node but current node is fine
        if (nextNode == null)
        {
            // Log.Debug("waiting for next node");
            return;
        }

        // Interpolation

        float t = (currentTime - firstNode.Value.Time) / (nextNode.Value.Time - firstNode.Value.Time);
        Vector3 position = Vector3.Lerp(firstNode.Value.Data.Position.ToUnity(), nextNode.Value.Data.Position.ToUnity(), t);

        Quaternion rotation = Quaternion.Lerp(firstNode.Value.Data.Rotation.ToUnity(), nextNode.Value.Data.Rotation.ToUnity(), t);

        transform.position = position;
        transform.rotation = rotation;
        // TODO: fix remote players being able to go through the object

        // Log.Debug($"moved {t} to {nextNode.Value.Data.Position.ToUnity()}");
    }

    public void DebugForward()
    {
        float currentTime = DayNightCycle.main.timePassedAsFloat;

        int count = 90;
        Log.Debug($"Adding {count} snapshots from {currentTime}");
        float delta = 20f / count;
        for (int i = 0; i < count; i++)
        {
            Vector3 result = transform.position + new Vector3(delta * i, 0, 0);

            MovementData movementData = new(null, result.ToDto(), transform.rotation.ToDto());

            AddSnapshot(movementData, currentTime + i * MovementBroadcaster.BROADCAST_PERIOD);
        }
    }

    public void DebugForwardRight()
    {
        float currentTime = (float)this.Resolve<TimeManager>().CurrentTime;

        int count = 90;
        float delta = 20f / 90f;
        float qDelta = 180f / 90f;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new(delta * i, 0, 0);
            Quaternion qOffset = Quaternion.AngleAxis(qDelta * i, transform.up);

            MovementData movementData = new(null, (transform.position + offset).ToDto(), (transform.rotation * qOffset).ToDto());
 
            AddSnapshot(movementData, currentTime + i * MovementBroadcaster.BROADCAST_PERIOD);
        }
    }

    private record struct Snapshot(MovementData Data, float Time)
    {
        public bool IsOlderThan(float currentTime) => currentTime < Time;
        
        public bool IsExpired(float currentTime) => currentTime > Time + SNAPSHOT_EXPIRATION_TIME;
    }
}

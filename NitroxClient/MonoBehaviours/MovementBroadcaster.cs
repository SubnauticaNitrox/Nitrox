using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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
        foreach (KeyValuePair<NitroxId, WatchedEntry> entry in watchedEntries)
        {
            // TODO: Don't broadcast at certain times: while docking, while docked ...
            data.Add(entry.Value.GetMovementData(entry.Key));
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
            Instance.watchedEntries.Add(entityId, new(gameObject.transform));
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

    private readonly record struct WatchedEntry
    {
        // TODO: eventually add a detector for multiple broadcast in a row where the watched entry has almost not moved
        // in this case, only send the data once every 5 second or as soon as new movement is detected
        private readonly Transform transform;
        private readonly Vehicle vehicle;
        private readonly SubControl subControl;

        public WatchedEntry(Transform transform)
        {
            this.transform = transform;
            vehicle = transform.GetComponent<Vehicle>();
            subControl = transform.GetComponent<SubControl>();
        }

        public MovementData GetMovementData(NitroxId id)
        {
            // Packets should be filled with more data if the vehicle is being driven by the local player
            if (vehicle && Player.main.currentMountedVehicle == vehicle)
            {
                // Those two values are set between -1 and 1 so we can easily scale them up while still in range for sbyte
                sbyte steeringWheelYaw = (sbyte)(Mathf.Clamp(vehicle.steeringWheelYaw, -1, 1) * 70f);
                sbyte steeringWheelPitch = (sbyte)(Mathf.Clamp(vehicle.steeringWheelPitch, -1, 1) * 45f);

                bool throttleApplied = false;

                Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                // See SeaMoth.UpdateSounds
                if (vehicle is SeaMoth)
                {
                    throttleApplied = input.magnitude > 0f;
                }
                // See Exosuit.Update
                else if (vehicle is Exosuit)
                {
                    throttleApplied = input.y > 0f;
                }

                return new DrivenVehicleMovementData(id, transform.position.ToDto(), transform.rotation.ToDto(), steeringWheelYaw, steeringWheelPitch, throttleApplied);
            }

            // TODO: find out if this is enough to ensure local player is piloting the said cyclops
            if (subControl && Player.main.currentSub == subControl.sub && Player.main.mode == Player.Mode.Piloting)
            {
                // Cyclop steering wheel's yaw and pitch are between -90 and 90 so they're already in range for sbyte
                sbyte steeringWheelYaw = (sbyte)Mathf.Clamp(subControl.steeringWheelYaw, -90, 90);
                sbyte steeringWheelPitch = (sbyte)Mathf.Clamp(subControl.steeringWheelPitch, -90, 90);
                
                // See SubControl.Update
                bool throttleApplied = subControl.throttle.magnitude > 0.0001f;

                return new DrivenVehicleMovementData(id, transform.position.ToDto(), transform.rotation.ToDto(), steeringWheelYaw, steeringWheelPitch, throttleApplied);
            }

            // Normal case in which the vehicule isn't driven by the local player
            return new SimpleMovementData(id, transform.position.ToDto(), transform.rotation.ToDto());
        }
    }
}

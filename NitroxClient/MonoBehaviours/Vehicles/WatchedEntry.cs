using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class WatchedEntry
{
    /// <remarks>
    /// In unity position units. Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MINIMAL_MOVEMENT_TRESHOLD = 0.05f;
    /// <remarks>
    /// In degrees (°). Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MINIMAL_ROTATION_TRESHOLD = 0.05f;
    /// <remarks>
    /// In seconds. Refer to <see cref="ShouldBroadcastMovement"/> for use infos.
    /// </remarks>
    private const float MAX_TIME_WITHOUT_BROADCAST = 5f;
    /// <inheritdoc cref="MAX_TIME_WITHOUT_BROADCAST"/>
    private const float SAFETY_BROADCAST_WINDOW = 0.2f;

    private readonly NitroxId Id;
    private readonly Transform transform;
    private readonly Vehicle vehicle;
    private readonly SubControl subControl;

    private float latestBroadcastTime;
    private Vector3 latestLocalPositionSent;
    private Quaternion latestLocalRotationSent;

    public WatchedEntry(NitroxId Id, Transform transform)
    {
        this.Id = Id;
        this.transform = transform;
        vehicle = transform.GetComponent<Vehicle>();
        subControl = transform.GetComponent<SubControl>();
    }

    private bool IsDrivenVehicle()
    {
        return vehicle && Player.main.currentMountedVehicle == vehicle;
    }

    private bool IsDrivenCyclops()
    {
        return subControl && Player.main.currentSub == subControl.sub && Player.main.mode == Player.Mode.Piloting;
    }

    public MovementData GetMovementData(NitroxId id)
    {
        // Packets should be filled with more data if the vehicle is being driven by the local player
        if (IsDrivenVehicle())
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

        if (IsDrivenCyclops())
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

    public void OnBroadcastPosition()
    {
        latestLocalPositionSent = transform.localPosition;
        latestLocalRotationSent = transform.localRotation;
    }

    private bool HasVehicleMoved()
    {
        return Vector3.Distance(latestLocalPositionSent, transform.localPosition) > MINIMAL_MOVEMENT_TRESHOLD ||
               Quaternion.Angle(latestLocalRotationSent, transform.localRotation) > MINIMAL_ROTATION_TRESHOLD;
    }

    /// <summary>
    /// Rate limiter which prevents all non-moving vehicles from sending too many packets following some rules:
    /// - the driven vehicle is not rate limited
    /// - position changes less than <see cref="MINIMAL_MOVEMENT_TRESHOLD"/> are ignored
    /// - rotation changes less than <see cref="MINIMAL_ROTATION_TRESHOLD"/> are ignored
    /// - every period of <see cref="MAX_TIME_WITHOUT_BROADCAST"/>, there's a <see cref="SAFETY_BROADCAST_WINDOW"/>
    /// during which movements packets are sent to avoid any packet drop's bad effect, regardless of <see cref="HasVehicleMoved"/>
    /// </summary>
    /// <remarks>
    /// <see cref="latestBroadcastTime"/> is not updated during the <see cref="SAFETY_BROADCAST_WINDOW"/> so we can recognize this window
    /// </remarks>
    public bool ShouldBroadcastMovement()
    {
        // Watched entry validity check (e.g. for vehicle death)
        if (!transform)
        {
            MovementBroadcaster.UnregisterWatched(Id);
            return false;
        }

        float deltaTimeSinceBroadcast = DayNightCycle.main.timePassedAsFloat - latestBroadcastTime;

        if (IsDrivenCyclops() || IsDrivenVehicle() || deltaTimeSinceBroadcast < 0 || HasVehicleMoved())
        {
            // As long as the vehicle has moved, we can reset the broadcast timer
            latestBroadcastTime = DayNightCycle.main.timePassedAsFloat;
            return true;
        }

        if (deltaTimeSinceBroadcast > MAX_TIME_WITHOUT_BROADCAST)
        {
            if (deltaTimeSinceBroadcast > MAX_TIME_WITHOUT_BROADCAST + SAFETY_BROADCAST_WINDOW)
            {
                // only reset the broadcast timer after the safety window has elapsed
                latestBroadcastTime = DayNightCycle.main.timePassedAsFloat;
            }
            return true;
        }

        return false;
    }
}

using System.Collections.Generic;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCamera_HandleInput_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.HandleInput());

    private static readonly Dictionary<NitroxId, CameraFrameState> preInputStatesByCameraId = [];

    private static bool previousLightState;

    public static bool Prefix(MapRoomCamera __instance)
    {
        previousLightState = __instance.lightsParent && __instance.lightsParent.activeInHierarchy;

        if (!__instance.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(cameraId))
        {
            preInputStatesByCameraId.Remove(cameraId);
            return true;
        }

        // No movement ownership:
        // - allow passive viewer input such as mouse-wheel camera switching
        // - block only actual camera-driving input so the viewer cannot visually drift/snap back
        if (HasBlockedControlInput())
        {
            MapRoomCamera_ControlCamera_Patch.ShowInUseStatus();

            // Keep trying to acquire ownership in the background.
            MapRoomCamera_ControlCamera_Patch.RequestControl(__instance, null, "HandleInputViewerRetry", false);

            return false;
        }

        preInputStatesByCameraId[cameraId] = new CameraFrameState(__instance);
        return true;
    }

    public static void Postfix(MapRoomCamera __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            return;
        }

        bool hasMovementOwnership = Resolve<SimulationOwnership>().HasExclusiveLock(cameraId);

        if (!hasMovementOwnership)
        {
            if (preInputStatesByCameraId.TryGetValue(cameraId, out CameraFrameState preInputState))
            {
                preInputState.Restore(__instance);
                preInputStatesByCameraId.Remove(cameraId);
            }

            // While viewing a camera without movement ownership, keep trying to acquire control at
            // a throttled rate. This lets a viewer automatically become controller after the old
            // owner exits/switches away, and gives late-join loose cameras a clean path to recover
            // from stale ownership state without needing to switch away and back.
            MapRoomCamera_ControlCamera_Patch.RequestControl(__instance, null, "HandleInputViewerRetry", false);

            return;
        }

        preInputStatesByCameraId.Remove(cameraId);

        if (PacketSuppressor<MapRoomCameraLightChanged>.IsSuppressed)
        {
            return;
        }

        bool currentLightState = __instance.lightsParent && __instance.lightsParent.activeInHierarchy;
        if (currentLightState == previousLightState)
        {
            return;
        }

        Resolve<IPacketSender>().Send(new MapRoomCameraLightChanged(cameraId, currentLightState));
    }

    private static bool HasBlockedControlInput()
    {
        if (GameInput.GetMoveDirection().sqrMagnitude > 0.0001f)
        {
            return true;
        }

        return UnityEngine.Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.001f ||
               UnityEngine.Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.001f;
    }

    private readonly struct CameraFrameState
    {
        private readonly Vector3 position;
        private readonly Quaternion rotation;
        private readonly Vector3 velocity;
        private readonly Vector3 angularVelocity;
        private readonly bool hadRigidBody;
        private readonly bool rigidBodyWasKinematic;

        public CameraFrameState(MapRoomCamera camera)
        {
            position = camera.transform.position;
            rotation = camera.transform.rotation;

            hadRigidBody = camera.rigidBody;
            if (camera.rigidBody)
            {
                velocity = camera.rigidBody.velocity;
                angularVelocity = camera.rigidBody.angularVelocity;
                rigidBodyWasKinematic = camera.rigidBody.isKinematic;
            }
            else
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                rigidBodyWasKinematic = false;
            }
        }

        public void Restore(MapRoomCamera camera)
        {
            if (!camera)
            {
                return;
            }

            camera.transform.SetPositionAndRotation(position, rotation);

            if (hadRigidBody && camera.rigidBody)
            {
                camera.rigidBody.position = position;
                camera.rigidBody.rotation = rotation;
                camera.rigidBody.velocity = velocity;
                camera.rigidBody.angularVelocity = angularVelocity;
                camera.rigidBody.isKinematic = rigidBodyWasKinematic;
            }
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCamera_ControlCamera_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.ControlCamera(default));

    private const float CONTROL_REQUEST_RETRY_SECONDS = 0.75f;
    private const float STATUS_MESSAGE_DURATION_SECONDS = 3f;
    private const float MOVEMENT_DENIED_STATUS_DURATION_SECONDS = 2.5f;

    private static readonly HashSet<NitroxId> pendingControlLocks = [];
    private static readonly Dictionary<NitroxId, float> lastControlRequestTimes = [];

    public static bool Prefix(MapRoomCamera __instance, MapRoomScreen screen)
    {
        MapRoomCameraPlayerAnchor.Begin(
            Player.main.transform.position,
            MainCameraControl.main.viewModel.transform.rotation,
            Player.main.camRoot.GetAimingTransform().rotation);

        RequestControl(__instance, screen, "ControlCamera", true);

        // Do not block vanilla camera entry.
        // Blocking here caused fake-box / broken-HUD / connecting-to-camera failures.
        // Movement authority is enforced in MapRoomCamera_HandleInput_Patch.
        return true;
    }

    public static void RequestControl(MapRoomCamera camera, MapRoomScreen screen, string reason, bool showStatus)
    {
        if (!camera)
        {
            return;
        }

        if (!camera.TryGetNitroxId(out NitroxId cameraId) || cameraId == null)
        {
            Log.Warn($"SCANNER_CAMERA_CONTROL_REQUEST_SKIP reason=noNitroxId source={reason} camera={camera.gameObject.GetFullHierarchyPath()}");
            return;
        }

        SimulationOwnership simulationOwnership = Resolve<SimulationOwnership>();

        if (simulationOwnership.HasExclusiveLock(cameraId))
        {
            EntityPositionBroadcaster.StopWatchingEntity(cameraId);
            MovementBroadcaster.RegisterWatched(camera.gameObject, cameraId);

            if (showStatus)
            {
                ShowAvailableStatus();
            }

            return;
        }

        if (pendingControlLocks.Contains(cameraId))
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (lastControlRequestTimes.TryGetValue(cameraId, out float lastRequestTime) &&
            now - lastRequestTime < CONTROL_REQUEST_RETRY_SECONDS)
        {
            return;
        }

        lastControlRequestTimes[cameraId] = now;
        pendingControlLocks.Add(cameraId);

        LockRequest<MapRoomCameraControl> lockRequest = new(
            cameraId,
            SimulationLockType.EXCLUSIVE,
            OnCameraControlLockResponse,
            new MapRoomCameraControl(camera, screen, reason, showStatus));

        simulationOwnership.RequestSimulationLock(lockRequest);

        if (ShouldLogControlEvent(reason))
        {
            Log.Info(
                $"SCANNER_CAMERA_CONTROL_REQUEST cameraId={cameraId} " +
                $"number={camera.cameraNumber} reason={reason} " +
                $"camera={camera.gameObject.GetFullHierarchyPath()} " +
                $"pendingCount={pendingControlLocks.Count}");
        }
    }

    public static void ShowInUseStatus()
    {
        MapRoomCameraStatusOverlay.ShowInUse(MOVEMENT_DENIED_STATUS_DURATION_SECONDS);
    }

    private static void ShowAvailableStatus()
    {
        MapRoomCameraStatusOverlay.ShowAvailable(STATUS_MESSAGE_DURATION_SECONDS);
    }

    private static void OnCameraControlLockResponse(NitroxId cameraId, bool lockAcquired, MapRoomCameraControl context)
    {
        pendingControlLocks.Remove(cameraId);

        if (!context.Camera)
        {
            Log.Info($"SCANNER_CAMERA_CONTROL_RESPONSE_IGNORED cameraId={cameraId} acquired={lockAcquired} reason={context.Reason} cause=missingCamera");
            return;
        }

        if (!lockAcquired)
        {
            EntityPositionBroadcaster.StopWatchingEntity(cameraId);
            MovementBroadcaster.UnregisterWatched(cameraId);

            if (ShouldLogControlEvent(context.Reason))
            {
                Log.Info(
                    $"SCANNER_CAMERA_CONTROL_DENIED cameraId={cameraId} " +
                    $"number={context.Camera.cameraNumber} reason={context.Reason} " +
                    $"camera={context.Camera.gameObject.GetFullHierarchyPath()}");
            }

            if (context.ShowStatus)
            {
                MapRoomCameraStatusOverlay.ShowInUse(STATUS_MESSAGE_DURATION_SECONDS);
            }

            return;
        }

        EntityPositionBroadcaster.StopWatchingEntity(cameraId);
        MovementBroadcaster.RegisterWatched(context.Camera.gameObject, cameraId);

        if (ShouldLogControlEvent(context.Reason))
        {
            Log.Info(
                $"SCANNER_CAMERA_CONTROL_GRANTED cameraId={cameraId} " +
                $"number={context.Camera.cameraNumber} reason={context.Reason} " +
                $"camera={context.Camera.gameObject.GetFullHierarchyPath()}");
        }

        // Show AVAILABLE when the player initially switches to a free camera, and also when
        // a viewer's background retry succeeds after the previous controller releases control.
        if (context.ShowStatus || context.Reason == "HandleInputViewerRetry")
        {
            ShowAvailableStatus();
        }
    }

    private static bool ShouldLogControlEvent(string reason)
    {
        return reason != "HandleInputViewerRetry";
    }
}

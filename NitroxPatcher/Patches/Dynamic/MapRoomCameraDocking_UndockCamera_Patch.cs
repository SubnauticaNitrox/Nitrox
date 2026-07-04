using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCameraDocking_UndockCamera_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCameraDocking t) => t.UndockCamera());

    private static MapRoomCamera undockedCamera;

    public static void Prefix(MapRoomCameraDocking __instance)
    {
        undockedCamera = __instance.camera;
    }

    public static void Postfix(MapRoomCameraDocking __instance)
    {
        if (PacketSuppressor<MapRoomCameraDockingChanged>.IsSuppressed)
        {
            return;
        }

        MapRoomFunctionality mapRoomFunctionality = __instance.GetComponentInParent<MapRoomFunctionality>(true);
        if (!mapRoomFunctionality)
        {
            return;
        }

        if (!mapRoomFunctionality.TryGetNitroxId(out NitroxId mapRoomId))
        {
            return;
        }

        List<MapRoomCameraDocking> dockings = mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>(true)
                                                                  .OrderBy(docking => docking.gameObject.GetFullHierarchyPath())
                                                                  .ToList();

        int dockingIndex = dockings.IndexOf(__instance);
        if (dockingIndex < 0)
        {
            return;
        }

        NitroxId cameraId = null;

        if (undockedCamera)
        {
            undockedCamera.TryGetNitroxId(out cameraId);
        }

        Resolve<IPacketSender>().Send(new MapRoomCameraDockingChanged(mapRoomId, dockingIndex, false, cameraId));

        if (undockedCamera)
        {
            MapRoomCameraIdentity.RequestCameraNumber(undockedCamera);
        }

        if (cameraId != null && undockedCamera)
        {
            SimulationOwnership simulationOwnership = Resolve<SimulationOwnership>();

            if (simulationOwnership.HasExclusiveLock(cameraId))
            {
                // The local player was already controlling this camera before it undocked.
                // Keep broadcasting so stalker theft / forced undock does not steal control away.
                EntityPositionBroadcaster.StopWatchingEntity(cameraId);
                MovementBroadcaster.RegisterWatched(undockedCamera.gameObject, cameraId);

                Log.Info($"SCANNER_CAMERA_UNDOCK_KEEP_EXISTING_CONTROL cameraId={cameraId} number={undockedCamera.cameraNumber} camera={undockedCamera.gameObject.GetFullHierarchyPath()}");
            }
            else
            {
                // Do not claim EXCLUSIVE control just because vanilla/stalker undocked the camera.
                // But this client is the one currently simulating the loose camera, so it needs a
                // TRANSIENT lock so the server will accept movement packets and late joiners get
                // the real world position instead of a stale copy.
                EntityPositionBroadcaster.StopWatchingEntity(cameraId);
                MovementBroadcaster.UnregisterWatched(cameraId);

                // Do not start MovementBroadcaster yet.
                // The server rejects VehicleMovements until the TRANSIENT lock is granted.
                // SimulationOwnership.TreatMapRoomCameraEntity(...) will register MovementBroadcaster
                // after the lock response is accepted.
                Resolve<SimulationOwnership>().RequestSimulationLock(cameraId, SimulationLockType.TRANSIENT);

                Log.Info($"SCANNER_CAMERA_UNDOCK_PASSIVE_TRANSIENT_REQUESTED cameraId={cameraId} number={undockedCamera.cameraNumber} camera={undockedCamera.gameObject.GetFullHierarchyPath()}");
            }
        }

        MapRoomCameraIdentity.NormalizeCameraNumbers();

        undockedCamera = null;
    }
}

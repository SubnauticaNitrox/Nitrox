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

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCameraDocking_DockCamera_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCameraDocking t) => t.DockCamera(default));

    public static void Postfix(MapRoomCameraDocking __instance, MapRoomCamera camera)
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
        camera.TryGetNitroxId(out cameraId);

        Resolve<IPacketSender>().Send(new MapRoomCameraDockingChanged(mapRoomId, dockingIndex, true, cameraId));

        if (cameraId != null)
        {
            MapRoomCameraIdentity.RequestCameraNumber(camera, mapRoomId, dockingIndex);
        }

        // Do not release movement ownership here.
        // Vanilla keeps the player connected to the docked camera after docking and performs its own
        // docked-camera view behavior. Releasing ownership here causes HandleInput to treat the local
        // player as view-only and restore the camera transform every frame, freezing the view at the dock.
        // Ownership is released by MapRoomCamera_ExitLockedMode_Patch when the player actually exits.
        MapRoomCameraIdentity.NormalizeCameraNumbers();
    }
}

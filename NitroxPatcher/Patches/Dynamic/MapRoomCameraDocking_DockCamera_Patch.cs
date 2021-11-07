using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class MapRoomCameraDocking_DockCamera_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD_DOCK = Reflect.Method((MapRoomCameraDocking t) => t.DockCamera(default(MapRoomCamera)));
        private static readonly MethodInfo TARGET_METHOD_UNDOCK = Reflect.Method((MapRoomCameraDocking t) => t.UndockCamera());
        private static IPacketSender packetSender;
        private static Building building;

        public static void PostfixDock(MapRoomCameraDocking __instance, MapRoomCamera camera)
        {
            UpdateMetadata(__instance, true, camera.GetComponent<NitroxEntity>().Id);
        }

        public static void PostfixUndock(MapRoomCameraDocking __instance)
        {
            packetSender ??= Resolve<IPacketSender>();
            packetSender.Send(new DockingStateChange(NitroxEntity.GetId(__instance.gameObject), false));
            UpdateMetadata(__instance, false);
        }

        private static void UpdateMetadata(MapRoomCameraDocking __instance, bool cameraDocked, NitroxId nitroxId = null)
        {
            building ??= Resolve<Building>();
            // Log.Debug($"Updating metadata for [{__instance.gameObject.name}, cameraDocked: {cameraDocked}]");
            // MRF_GO is the MapRoomFunctionality's GameObject containing dockPoint1 and dockPoint2
            GameObject MRF_GO = __instance.transform.parent.gameObject;
            GameObject baseParent = MRF_GO.transform.parent.gameObject;
            // NitroxEntity of the MapRoomFunctionality which is the direct parent of the docking
            NitroxId mapRoomFunctionalityId = NitroxEntity.GetId(MRF_GO);
            MapRoomMetadata cameraDockingMetadata = new(mapRoomFunctionalityId, false, false, null, null);
            // Verify which docking point is __instance then set CameraDocked to true if a camera is docked to it
            if (__instance.gameObject.name.Equals("dockingPoint1"))
            {
                MapRoomCameraDocking otherDocking = MRF_GO.FindChild("dockingPoint2").GetComponent<MapRoomCameraDocking>();
                cameraDockingMetadata.CameraDocked1 = cameraDocked;
                cameraDockingMetadata.CameraDocked2 = otherDocking.cameraDocked;
                cameraDockingMetadata.Camera1NitroxId = (cameraDocked ? nitroxId : null);
                if (otherDocking.cameraDocked && otherDocking.camera.TryGetComponent(out NitroxEntity otherNitroxEntity))
                {
                    cameraDockingMetadata.Camera2NitroxId = otherNitroxEntity.Id;
                }
            }
            else if (__instance.gameObject.name.Equals("dockingPoint2"))
            {
                MapRoomCameraDocking otherDocking = MRF_GO.FindChild("dockingPoint1").GetComponent<MapRoomCameraDocking>();
                cameraDockingMetadata.CameraDocked2 = cameraDocked;
                cameraDockingMetadata.CameraDocked1 = otherDocking.cameraDocked;
                cameraDockingMetadata.Camera2NitroxId = (cameraDocked ? nitroxId : null);
                if (otherDocking.cameraDocked && otherDocking.camera.TryGetComponent(out NitroxEntity otherNitroxEntity))
                {
                    cameraDockingMetadata.Camera1NitroxId = otherNitroxEntity.Id;
                }
            }

            // If we don't refresh the payload, an empty one will be sent
            cameraDockingMetadata.RefreshUpdatePayload();
            // Log.Debug($"Sending MetadataChanged packet [{cameraDockingMetadata}]");
            building.MetadataChanged(NitroxEntity.GetId(baseParent), NitroxEntity.GetId(MRF_GO), cameraDockingMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD_DOCK, nameof(PostfixDock));
            PatchPostfix(harmony, TARGET_METHOD_UNDOCK, nameof(PostfixUndock));
        }
    }
}


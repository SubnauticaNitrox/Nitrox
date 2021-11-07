using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class uGUI_MapRoomScanner_OnCancelScan_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_MapRoomScanner t) => t.OnCancelScan());
        private static IPacketSender packetSender;
        private static Building building;

        public static void Postfix(uGUI_MapRoomScanner __instance)
        {
            packetSender ??= Resolve<IPacketSender>();
            building ??= Resolve<Building>();
            GameObject MRF_GO = __instance.transform.parent.parent.gameObject;
            NitroxId mapRoomFunctionalityId = NitroxEntity.GetId(MRF_GO);

            building.MetadataChanged(NitroxEntity.GetId(MRF_GO.transform.parent.gameObject), mapRoomFunctionalityId, new MapRoomMetadata(mapRoomFunctionalityId, null));
            packetSender.Send(new MapRoomScan(NitroxEntity.GetId(__instance.gameObject), false));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

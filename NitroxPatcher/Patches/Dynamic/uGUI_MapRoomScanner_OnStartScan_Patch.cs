using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class uGUI_MapRoomScanner_OnStartScan_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_MapRoomScanner t) => t.OnStartScan(default(int)));
        private static IPacketSender packetSender;
        private static Building building;

        public static void Postfix(uGUI_MapRoomScanner __instance)
        {
            packetSender ??= Resolve<IPacketSender>();
            building ??= Resolve<Building>();
            NitroxTechType newTypeToScan = __instance.mapRoom.typeToScan.ToDto();
            GameObject MRF_GO = __instance.transform.parent.parent.gameObject;
            NitroxId mapRoomFunctionalityId = NitroxEntity.GetId(MRF_GO);

            building.MetadataChanged(NitroxEntity.GetId(MRF_GO.transform.parent.gameObject), mapRoomFunctionalityId, new MapRoomMetadata(mapRoomFunctionalityId, newTypeToScan));
            packetSender.Send(new MapRoomScan(NitroxEntity.GetId(__instance.gameObject), newTypeToScan, true));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

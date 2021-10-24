using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnDeselect());

        public static void Postfix(uGUI_SignInput __instance)
        {
            GameObject gameObject = __instance.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);

            SignMetadata signMetadata = new(__instance.text, __instance.colorIndex, __instance.scaleIndex, __instance.elementsState, __instance.IsBackground());
            NitroxServiceLocator.LocateService<Building>().MetadataChanged(id, signMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

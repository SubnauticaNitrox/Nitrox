using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;
using UnityEngine;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_SignInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnDeselect", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(uGUI_SignInput __instance)
        {
            GameObject gameObject = __instance.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);

            SignMetadata signMetadata = new SignMetadata(__instance.text, __instance.colorIndex, __instance.scaleIndex, __instance.elementsState, __instance.IsBackground());
            NitroxServiceLocator.LocateService<Building>().MetadataChanged(id, signMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

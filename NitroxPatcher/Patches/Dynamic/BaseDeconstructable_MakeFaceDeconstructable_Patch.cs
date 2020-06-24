using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseDeconstructable_MakeFaceDeconstructable_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("MakeFaceDeconstructable");

        public static void Postfix(BaseDeconstructable __instance, Transform geometry)
        {
            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().ReassignPreservedNitroxIdsAndNewIds(__instance.gameObject.GetComponentInParent<Base>(), geometry);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

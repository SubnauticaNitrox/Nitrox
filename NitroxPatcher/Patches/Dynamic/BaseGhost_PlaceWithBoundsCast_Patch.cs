using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseGhost_PlaceWithBoundsCast_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("PlaceWithBoundsCast", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(BaseGhost __instance, ref bool __result, ref Vector3 center)
        {
            NitroxServiceLocator.LocateService<Building>().BaseGhost_PlaceWithBoundsCast_Post(__instance, ref __result, ref center);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

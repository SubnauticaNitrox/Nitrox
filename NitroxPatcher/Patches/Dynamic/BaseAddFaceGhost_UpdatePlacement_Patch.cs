using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseAddFaceGhost_UpdatePlacement_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseAddFaceGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdatePlacement", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(BaseAddModuleGhost __instance, ref bool __result, Transform camera, float placeMaxDistance, ref bool positionFound, ref bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
        {
            return NitroxServiceLocator.LocateService<Building>().BaseAddFaceGhost_UpdatePlacement_Pre(__instance, ref __result, camera, placeMaxDistance, ref positionFound, ref geometryChanged, ghostModelParentConstructableBase);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

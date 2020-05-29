using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    class BaseHullStrength_OnPostRebuildGeometry_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseHullStrength);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPostRebuildGeometry", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(BaseHullStrength __instance, Base b)
        {
            return NitroxServiceLocator.LocateService<Building>().BaseHullStrength_OnPostRebuildGeometry_Pre(__instance, b);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

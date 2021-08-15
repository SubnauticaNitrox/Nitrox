using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Base_ClearGeometry_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Base);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ClearGeometry", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Base __instance)
        {
            if (__instance == null)
            {
                return;
            }

            NitroxServiceLocator.LocateService<GeometryRespawnManager>().GeometryClearedForBase(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

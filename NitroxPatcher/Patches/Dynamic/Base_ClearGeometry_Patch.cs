using System;
using System.Reflection;
using Harmony;
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
            if(__instance == null)
            {
                return;
            }

            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().Base_ClearGeometry_Pre(__instance);
        }       

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

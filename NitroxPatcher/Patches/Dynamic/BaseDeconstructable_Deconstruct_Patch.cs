using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseDeconstructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(BaseDeconstructable __instance)
        {
            NitroxServiceLocator.LocateService<Building>().BaseDeconstructable_Deconstruct_Pre(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

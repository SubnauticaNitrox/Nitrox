using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseGhost_Finish_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Finish", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(BaseGhost __instance)
        {
            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().BaseGhost_Finish_Pre(__instance);
        }

        public static void Postfix(BaseGhost __instance)
        {
            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().BaseGhost_Finish_Post(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}


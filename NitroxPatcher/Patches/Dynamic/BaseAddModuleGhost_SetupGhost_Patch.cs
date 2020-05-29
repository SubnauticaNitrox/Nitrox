using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseAddModuleGhost_SetupGhost_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseAddModuleGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetupGhost", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(BaseAddModuleGhost __instance)
        {
            return NitroxServiceLocator.LocateService<Building>().BaseAddModuleGhost_SetupGhost_Pre(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

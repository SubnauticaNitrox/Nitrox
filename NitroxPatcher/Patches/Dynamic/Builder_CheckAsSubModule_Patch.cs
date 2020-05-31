using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_CheckAsSubModule_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CheckAsSubModule", BindingFlags.NonPublic | BindingFlags.Static);

        public static bool Prefix(ref bool __result)
        {
            return NitroxServiceLocator.LocateService<Building>().Builder_CheckAsSubModule_Pre(ref __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

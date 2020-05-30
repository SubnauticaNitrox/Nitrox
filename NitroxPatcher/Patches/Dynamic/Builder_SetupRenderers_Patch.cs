using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_SetupRenderers_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetupRenderers", BindingFlags.NonPublic | BindingFlags.Static);

        public static bool Prefix(ref bool interior)
        {
            return NitroxServiceLocator.LocateService<Building>().Builder_SetupRenderers_Pre(ref interior);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

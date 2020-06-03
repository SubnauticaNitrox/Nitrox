using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    class Constructable_CheckFlags_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CheckFlags");

        public static bool Prefix(ref bool __result)
        {
            return NitroxServiceLocator.LocateService<Building>().Constructable_CheckFlags_Pre(ref __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static bool Prefix(Constructable __instance, ref bool __result)
        {
            return NitroxServiceLocator.LocateService<Building>().Constructable_Deconstruct_Pre(__instance, ref __result);
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().Constructable_Deconstruct_Post(__instance, __result);
            NitroxServiceLocator.LocateService<Building>().Constructable_Deconstruct_Post(__instance, __result);
        }
        
        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

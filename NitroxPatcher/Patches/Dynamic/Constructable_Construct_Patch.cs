using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Construct");

        public static bool Prefix(Constructable __instance, ref bool __result)
        {
            return NitroxServiceLocator.LocateService<Building>().Constructable_Construct_Pre(__instance, ref __result);
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            NitroxServiceLocator.LocateService<Building>().Constructable_Construct_Post(__instance, __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

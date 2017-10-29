using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class Creature_ChooseBestAction_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Creature);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ChooseBestAction", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(Creature __instance, ref CreatureAction __result)
        {
            __result = null;
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

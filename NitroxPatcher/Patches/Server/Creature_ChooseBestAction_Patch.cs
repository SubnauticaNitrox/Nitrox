using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class Creature_ChooseBestAction_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Creature);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ChooseBestAction", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(Creature __instance, out CreatureAction __state)
        {
            FieldInfo fieldInfo = typeof(Creature).GetField("prevBestAction", BindingFlags.NonPublic | BindingFlags.Instance);
            __state = (CreatureAction)fieldInfo.GetValue(__instance);

            return true;
        }

        public static void Postfix(Creature __instance, CreatureAction __result, CreatureAction __state)
        {
            if (__result != __state)
            {
                if(__result != null)
                {
                    NitroxServer.Server.Logic.AI.CreatureActionChanged(__instance.transform.position, __result);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

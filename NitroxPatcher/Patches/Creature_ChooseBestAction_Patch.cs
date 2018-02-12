using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class Creature_ChooseBestAction_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Creature);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ChooseBestAction", BindingFlags.NonPublic | BindingFlags.Instance);

        private static CreatureAction previousAction;

        public static bool Prefix(Creature __instance, ref CreatureAction __result)
        {
            string guid = GuidHelper.GetGuid(__instance.gameObject);

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasOwnership(guid))
            {
                previousAction = (CreatureAction)typeof(Creature).GetField("prevBestAction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                return true;
            }

            // CreatureActionChangedProcessor.ActionByGuid.TryGetValue(guid, out __result);

            return false;
        }

        public static void Postfix(Creature __instance, ref CreatureAction __result)
        {
            string guid = GuidHelper.GetGuid(__instance.gameObject);

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasOwnership(guid))
            {
                if (previousAction != __result)
                {
                    // Multiplayer.Logic.AI.CreatureActionChanged(guid, __result);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

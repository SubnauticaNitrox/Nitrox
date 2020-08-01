using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Creature_ChooseBestAction_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Creature);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ChooseBestAction", BindingFlags.NonPublic | BindingFlags.Instance);

        private static CreatureAction previousAction;

        public static bool Prefix(Creature __instance, ref CreatureAction __result)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(id))
            {
                previousAction = (CreatureAction)typeof(Creature).GetField("prevBestAction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                return true;
            }

            // CreatureActionChangedProcessor.ActionById.TryGetValue(id, out __result);

            return false;
        }

        public static void Postfix(Creature __instance, ref CreatureAction __result)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(id))
            {
                if (previousAction != __result)
                {
                    // Multiplayer.Logic.AI.CreatureActionChanged(id, __result);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

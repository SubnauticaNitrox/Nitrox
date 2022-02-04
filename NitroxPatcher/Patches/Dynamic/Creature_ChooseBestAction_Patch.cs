using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Creature_ChooseBestAction_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Creature t) => t.ChooseBestAction());

        private static CreatureAction previousAction;

        public static bool Prefix(Creature __instance, ref CreatureAction __result)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(id))
            {
                previousAction = __instance.prevBestAction;
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
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}

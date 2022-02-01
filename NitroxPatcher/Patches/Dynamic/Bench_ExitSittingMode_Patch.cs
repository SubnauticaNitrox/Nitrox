using System.Collections;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bench_ExitSittingMode_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bench t) => t.ExitSittingMode(default, default));

        public static void Prefix(ref bool __runOriginal)
        {
            __runOriginal = !Resolve<PlayerChatManager>().IsChatSelected && !DevConsole.instance.selected;
        }

        public static void Postfix(Bench __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return;
            }
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);

            // Request to be downgraded to a transient lock so we can still simulate the positioning.
            Resolve<SimulationOwnership>().RequestSimulationLock(id, SimulationLockType.TRANSIENT);

            Resolve<LocalPlayer>().AnimationChange(AnimChangeType.BENCH, AnimChangeState.OFF);
            __instance.StartCoroutine(ResetAnimationDelayed(__instance.standUpCinematicController.interpolationTimeOut));
        }

        private static IEnumerator ResetAnimationDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Resolve<LocalPlayer>().AnimationChange(AnimChangeType.BENCH, AnimChangeState.UNSET);
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
        }
    }
}

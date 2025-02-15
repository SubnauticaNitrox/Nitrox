using HarmonyLib;
using NitroxModel.Helper;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    /*
     * FROM:
     * 		bool flag = MiscSettings.pdaPause && (this.state == PDA.State.Opened || this.state == PDA.State.Opening || this.state == PDA.State.Closing);
		FreezeTime.Set(FreezeTime.Id.PDA, flag ? this.sequence.t : 0f);
		bool flag2 = FreezeTime.GetTopmostId() == FreezeTime.Id.PDA;
		bool flag3 = flag && flag2;
		PDA.UpdateTime(flag3);
		Player.main.playerAnimator.updateMode = (flag3 ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal);
    TO:
    PDA.UpdateTime(false); [REPLACED]
    Player.main.playerAnimator.updateMode = AnimatorUpdateMode.Normal; [REPLACED]
    */
    public sealed partial class PDA_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PDA t) => t.ManagedUpdate());

        public static bool Prefix(PDA __instance)
        {
            FieldInfo initializedField = Player.main.playerAnimator.GetType().GetField("updateMode", BindingFlags.NonPublic | BindingFlags.Instance);
            initializedField.SetValue(__instance, AnimatorUpdateMode.Normal);
            return true;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions).MatchStartForward().RemoveInstructions(45).Insert(
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => PDA.UpdateTime(false)))
                ).InstructionEnumeration();
        }
    }
}

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

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions).RemoveInstructions(45).Insert(
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => PDA.UpdateTime(false))),
                new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => Player.main)),
                new CodeInstruction(OpCodes.Ldfld, Reflect.Field((Player t) => t.playerAnimator)),
                new CodeInstruction(OpCodes.Call, Reflect.Method((Animator j) => Reflect.Property((Animator t) => t.updateMode).SetValue(j, AnimatorUpdateMode.Normal)))
                ).InstructionEnumeration();
        }
    }
}

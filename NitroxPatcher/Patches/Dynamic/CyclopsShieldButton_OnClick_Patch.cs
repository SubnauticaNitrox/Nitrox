using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsShieldButton_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsShieldButton t) => t.OnClick());
        public static readonly OpCode START_CUT_CODE = OpCodes.Ldsfld;
        public static readonly OpCode START_CUT_CODE_CALL = OpCodes.Callvirt;
        public static readonly FieldInfo PLAYER_MAIN_FIELD = Reflect.Field(() => Player.main);
        public static readonly OpCode END_CUT_CODE = OpCodes.Ret;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            int startCut = 0;
            int endCut = instructionList.Count;
            /* Cut out
             * if (Player.main.currentSub != this.subRoot)
             * {
             * 	return;
             * }
             */
            for (int i = 1; i < instructionList.Count; i++)
            {
                if (instructionList[i - 1].opcode.Equals(START_CUT_CODE) && instructionList[i - 1].operand.Equals(PLAYER_MAIN_FIELD) && instructionList[i].opcode == START_CUT_CODE_CALL)
                {
                    startCut = i - 1;
                }
                // Cut at the first return encountered
                if (endCut == instructionList.Count && instructionList[i].opcode.Equals(END_CUT_CODE))
                {
                    endCut = i;
                }
            }
            instructionList.RemoveRange(startCut, endCut + 1);
            if (startCut == 0)
            {
                instructionList.Insert(0, new CodeInstruction(OpCodes.Nop));
            }
            return instructionList;
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

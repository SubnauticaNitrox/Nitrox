using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class SubRoot_OnPlayerEntered_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnPlayerEntered(default(Player)));
        private static readonly OpCode START_INJECTION_CODE = OpCodes.Ldarg_0;
        private static readonly OpCode START_INJECTION_CODE_INVINCIBLE = OpCodes.Stfld;
        private static readonly FieldInfo LIVEMIXIN_INVINCIBLE = Reflect.Field((LiveMixin t) => t.invincible);

        /* There is a bug, where Subroot.live is not loaded when starting in a cyclops. Therefore this codepiece needs to check that and jump accordingly if not present
         * 
         * For this change
         * 
         * this.live.invincible = false
         * 
         * to
         * 
         * if (this.live != null)
         * {
         *  this.live.invincible = false
         * } 
         */
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            int injectionPoint = 0;
            Label newJumpPoint = generator.DefineLabel();
            for (int i = 3; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == START_INJECTION_CODE_INVINCIBLE &&
                    Equals(instructionList[i].operand, LIVEMIXIN_INVINCIBLE))
                {
                    if (instructionList[i - 3].opcode == START_INJECTION_CODE)
                    {
                        instructionList[i + 1].labels.Add(newJumpPoint);
                        injectionPoint = i - 3;
                    }
                }
            }
            if (injectionPoint != 0)
            {
                MethodInfo op_inequality_method = typeof(UnityEngine.Object).GetMethod("op_Inequality");
                List<CodeInstruction> injectedInstructions = new()
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,  Reflect.Field((SubRoot t) => t.live)),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Call, op_inequality_method),
                    new CodeInstruction(OpCodes.Brfalse, newJumpPoint)
                };
                instructionList.InsertRange(injectionPoint, injectedInstructions);
            }
            return instructionList;
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
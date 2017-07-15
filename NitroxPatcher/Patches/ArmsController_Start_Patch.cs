using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NitroxPatcher.Patches
{
    public class ArmsController_Start_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ArmsController);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
        public static readonly object INJECTION_OPERAND = typeof(ArmsController).GetField("pda", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPCODE);
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * ArmsController.Reconfigure(null);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldnull);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(ArmsController).GetMethod("Reconfigure", BindingFlags.NonPublic | BindingFlags.Instance));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

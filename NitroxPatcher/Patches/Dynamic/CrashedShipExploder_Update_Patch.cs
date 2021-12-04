using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CrashedShipExploder_Update_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashedShipExploder t) => t.Update());
        private PDAManagerEntry pdaManagerEntry = Resolve<PDAManagerEntry>();
        public static CrashedShipExploder_Update_Patch Instance;

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
        public static readonly object INJECTION_OPERAND = Reflect.Field((CrashedShipExploder t) => t.initialized);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            /* Modify the line
             * if (DayNightCycle.main != null)
             * to add another check, it prevents any explosion triggered by the client itself
             */
            CodeInstruction firstInstruction = new (OpCodes.Ldsfld, Reflect.Field(() => Instance));
            CodeInstruction methodInstruction = new (OpCodes.Call, Reflect.Method((CrashedShipExploder_Update_Patch t) => t.CrashedUpdate()));
            CodeInstruction brInstruction = new (OpCodes.Brfalse);

            int insertIndex = 0;
            foreach(CodeInstruction instruction in instructions)
            {
                insertIndex--;
                if (insertIndex == 1)
                {
                    brInstruction.operand = instruction.operand;
                }
                if (insertIndex == 0)
                {
                    foreach (CodeInstruction instructionToAdd in new List<CodeInstruction>() { firstInstruction, methodInstruction, brInstruction })
                    {
                        yield return instructionToAdd;
                    }
                }

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    insertIndex = 5;
                }
                yield return instruction;
            }
        }

        public bool CrashedUpdate()
        {
            pdaManagerEntry ??= Resolve<PDAManagerEntry>();
            return pdaManagerEntry != null && pdaManagerEntry.CrashedUpdate;
        }

        public override void Patch(Harmony harmony)
        {
            Instance = this;
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

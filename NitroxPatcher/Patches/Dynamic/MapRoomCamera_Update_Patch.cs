using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class MapRoomCamera_Update_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.Update());

        private static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        private static readonly object INJECTION_OPERAND = Reflect.Method((MapRoomCamera t) => t.CanBeControlled(default(MapRoomScreen)));

        private static readonly OpCode INJECTION_OPCODE_2 = OpCodes.Call;
        private static readonly object INJECTION_OPERAND_2 = Reflect.Method((MapRoomCamera t) => t.FreeCamera(default(bool)));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            Validate.NotNull(INJECTION_OPERAND_2);

            int addIndex = -1;

            CodeInstruction loadInstruction = new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => CameraControlManager.Instance));
            CodeInstruction callInstruction = new CodeInstruction(OpCodes.Call, Reflect.Method((CameraControlManager t) => t.HasControlOverCurrentCamera()));
            CodeInstruction brFalseInstruction = new CodeInstruction(OpCodes.Brfalse);
            foreach (CodeInstruction instruction in instructions)
            {
                addIndex--;
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    // We need to add it 1 line under (after the brfalse line)
                    addIndex = 1;
                }
                if (addIndex == 0)
                {
                    // This is the good place to add the code
                    brFalseInstruction.operand = instruction.operand;
                    foreach (CodeInstruction instructionToAdd in new List<CodeInstruction>(){ loadInstruction, callInstruction, brFalseInstruction })
                    {
                        yield return instructionToAdd;
                    }
                }

                if (instruction.opcode.Equals(INJECTION_OPCODE_2) && instruction.operand.Equals(INJECTION_OPERAND_2))
                {
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => MapRoomScreen_OnHandClick_Patch.ReleaseScreen()));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

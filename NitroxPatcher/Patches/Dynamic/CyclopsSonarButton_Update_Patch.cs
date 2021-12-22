using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsSonarButton_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.Update());

        private static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        private static readonly object INJECTION_OPERAND = Reflect.Property((CyclopsSonarButton t) => t.sonarActive).GetGetMethod(true);

        private static NitroxId currentCyclopsId;

        public static void Prefix(CyclopsSonarButton __instance)
        {
            currentCyclopsId = NitroxEntity.GetId(__instance.subRoot.gameObject);
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            /* Normally in the Update()
             * if (Player.main.GetMode() == Player.Mode.Normal && this.sonarActive)
		     * {
		     *     this.TurnOffSonar();
		     * }
             * this part will be changed:
             * if (Player.main.GetMode() == Player.Mode.Normal && this.sonarActive && CyclopsSonarButton_Update_Patch.ShouldTurnOff())
             */
            CodeInstruction callInstruction = new(OpCodes.Call, Reflect.Method(() => ShouldTurnoff()));
            CodeInstruction brInstruction = new(OpCodes.Brfalse);

            // There are 2 occurences of INJECTION_OPCODE and INJECTION_OPERAND
            // shouldAdd makes it so that the code is only injected the second time
            int addIndex = -1;
            bool shouldAdd = true;
            foreach (CodeInstruction instruction in instructions)
            {
                addIndex--;
                yield return instruction;
                
                if (addIndex == 0)
                {
                    shouldAdd = !shouldAdd;
                    if (!shouldAdd)
                    {
                        continue;
                    }
                    brInstruction.operand = instruction.operand;
                    foreach (CodeInstruction instructionToAdd in new List<CodeInstruction>() { callInstruction, brInstruction })
                    {
                        yield return instructionToAdd;
                    }
                }
                // When getting to this.sonarActive, we want to inject the code after the brfalse that follows this instruction
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    addIndex = 1;
                }
            }
        }

        public static bool ShouldTurnoff()
        {
            return Resolve<Cyclops>().ShouldSonarTurnoff(currentCyclopsId);
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix: true, transpiler: true);
        }

    }
}

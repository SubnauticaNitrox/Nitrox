using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents sonar from turning off automatically for the player that isn't currently piloting the Cyclops.
/// </summary>
public class CyclopsSonarButton_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.Update());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Ldsfld;
    internal static readonly object INJECTION_OPERAND = Reflect.Field(() => Player.main);

    private static NitroxId currentCyclopsId;

    public static void Prefix(CyclopsSonarButton __instance)
    {
        currentCyclopsId = NitroxEntity.GetId(__instance.subRoot.gameObject);
    }

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions)
    {
        /* Normally in the Update()
         * if (Player.main.GetMode() == Player.Mode.Normal && this.sonarActive)
         * {
         *     this.TurnOffSonar();
         * }
         * this part will be changed into:
         * if (CyclopsSonarButton_Update_Patch.ShouldTurnOff() && Player.main.GetMode() == Player.Mode.Normal && this.sonarActive)
         */
        List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);
        CodeInstruction callInstruction = new(OpCodes.Call, Reflect.Method(() => ShouldTurnoff()));
        CodeInstruction brInstruction = new(OpCodes.Brfalse);
        for (int i = 0; i < codeInstructions.Count; i++)
        {
            CodeInstruction instruction = codeInstructions[i];

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                // The second line after the current instruction should be a Brtrue, we need its operand to have the same jump label for our brfalse
                CodeInstruction nextBr = codeInstructions[i + 2];

                // The new instruction will be the first of the if statement, so it should take the jump labels that the former first part of the statement had
                callInstruction.labels = new List<Label>(instruction.labels);
                instruction.labels.Clear();
                brInstruction.operand = nextBr.operand;

                yield return callInstruction;
                yield return brInstruction;
            }
            yield return instruction;
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

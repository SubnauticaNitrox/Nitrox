using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents sonar from turning off automatically for the player that isn't currently piloting the Cyclops.
/// </summary>
public sealed partial class CyclopsSonarButton_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.Update());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Ldsfld;
    internal static readonly object INJECTION_OPERAND = Reflect.Field(() => Player.main);

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase methodBase, IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        /* Normally in the Update()
         * if (Player.main.GetMode() == Player.Mode.Normal && this.sonarActive)
         * {
         *     this.TurnOffSonar();
         * }
         * this part will be changed into:
         * if (CyclopsSonarButton_Update_Patch.ShouldTurnOff(this) && Player.main.GetMode() == Player.Mode.Normal && this.sonarActive)
         */
        List<CodeInstruction> codeInstructions = new(instructions);
        Label brLabel = il.DefineLabel();
        CodeInstruction loadInstruction = new(OpCodes.Ldarg_0);
        CodeInstruction callInstruction = new(OpCodes.Call, Reflect.Method(() => ShouldTurnoff(default)));
        CodeInstruction brInstruction = new(OpCodes.Brfalse, brLabel);
        codeInstructions.Last().labels.Add(brLabel);

        for (int i = 0; i < codeInstructions.Count; i++)
        {
            CodeInstruction instruction = codeInstructions[i];

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                // The second line after the current instruction should be a Brtrue, we need its operand to have the same jump label for our brfalse
                CodeInstruction nextBr = codeInstructions[i + 2];

                // The new instruction will be the first of the if statement, so it should take the jump labels that the former first part of the statement had
                instruction.MoveLabelsTo(loadInstruction);

                yield return loadInstruction;
                yield return callInstruction;
                yield return brInstruction;
            }
            yield return instruction;
        }
    }

    /// <returns>true (sonar should be turned off) if local player is simulating the cyclops (there's no replicator in this case)</returns>
    public static bool ShouldTurnoff(CyclopsSonarButton cyclopsSonarButton)
    {
        return !cyclopsSonarButton.subRoot.GetComponent<CyclopsMovementReplicator>();
    }
}

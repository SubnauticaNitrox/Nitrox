using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    /// <summary>
    /// The sonar will stay on until the player leaves the vehicle and automatically turns on when they enter again (if sonar was on at that time).
    /// </summary>
    public class CyclopsSonarButton_SonarPing_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.SonarPing());
        public static readonly OpCode JUMP_TARGET_CODE = OpCodes.Ldsfld;
        public static readonly FieldInfo JUMP_TARGET_FIELD = Reflect.Field(() => SNCameraRoot.main);


        // Send ping to other players        
        public static void Postfix(CyclopsSonarButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            Resolve<Cyclops>().BroadcastSonarPing(id);
        }


        /* As the ping would be always be executed it should be restricted to players, that are in the cyclops
        * Therefore the code generated from AssembleNewCode will be injected before the ping would be send but after energy consumption
        * End result:
        * private void SonarPing()
        * {
        * 	float num = 0f;
        * 	if (!this.subRoot.powerRelay.ConsumeEnergy(this.subRoot.sonarPowerCost, out num))
        * 	{
        * 	    this.TurnOffSonar();
        * 	    return;
        * 	}
        * 	if(Player.main.currentSub != this.subroot)
        * 	{
        * 	    return;
        * 	}
        * 	SNCameraRoot.main.SonarPing();
        * 	this.soundFX.Play();
        * }
        */
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            // Need to change the jump target for Brtrue at one point
            Label toInjectJump = iLGenerator.DefineLabel();

            // Find point to inject if player is in subroot:
            // SNCameraRoot.main.SonarPing();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode.Equals(JUMP_TARGET_CODE) && instruction.operand.Equals(JUMP_TARGET_FIELD))
                {
                    Label jumpLabel = instruction.labels[0];
                    IEnumerable<CodeInstruction> injectInstructions = AssembleNewCode(jumpLabel, toInjectJump);
                    foreach (CodeInstruction injectInstruction in injectInstructions)
                    {
                        yield return injectInstruction;
                    }
                }

                /* New jump target will from 
                 * 
                 * if (!this.subRoot.powerRelay.ConsumeEnergy(this.subRoot.sonarPowerCost, out num))
                 * 
                 * will be new code
                 */
                if (instruction.opcode.Equals(OpCodes.Brtrue))
                {
                    if (instructionList[i - 1].opcode.Equals(OpCodes.Call) && instructionList[i + 1].opcode.Equals(OpCodes.Ldarg_0))
                    {
                        instruction.operand = toInjectJump;
                    }
                }
                yield return instruction;
            }
        }

        private static IEnumerable<CodeInstruction> AssembleNewCode(Label outJumpLabel, Label innerJumpLabel)
        {
            //Code to inject:
            /*
             * if (Player.main.currentSub != this.subRoot)
		     * {
			 *  return;
		     * }
             * 
             */
            List<CodeInstruction> injectInstructions = new();

            CodeInstruction instruction = new(OpCodes.Ldsfld);
            instruction.operand = Reflect.Field(() => Player.main);
            instruction.labels.Add(innerJumpLabel);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Callvirt);
            instruction.operand = Reflect.Property((Player t) => t.currentSub).GetMethod;
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Ldarg_0);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Ldfld);
            instruction.operand = Reflect.Field((CyclopsSonarButton t) => t.subRoot);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Call);
            // Reflect utility class does not supported getting operator-overload methods.
            instruction.operand = typeof(UnityEngine.Object).GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Brfalse);
            instruction.operand = outJumpLabel;
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Ret);
            injectInstructions.Add(instruction);

            return injectInstructions;
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, postfix: true, transpiler: true);
        }
    }
}

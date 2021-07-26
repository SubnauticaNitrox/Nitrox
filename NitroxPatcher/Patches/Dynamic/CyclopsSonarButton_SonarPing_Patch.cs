using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsSonarButton_SonarPing_Patch : NitroxPatch, IDynamicPatch
    {

        public static readonly Type TARGET_CLASS = typeof(CyclopsSonarButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SonarPing", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly OpCode JUMP_TARGET_CODE = OpCodes.Ldsfld;
        public static readonly FieldInfo JUMP_TARGET_FIELD = typeof(SNCameraRoot).GetField("main", BindingFlags.Public | BindingFlags.Static);

        
        // Send ping to other players        
        public static void Postfix(CyclopsSonarButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastSonarPing(id);
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
                if(instruction.opcode.Equals(JUMP_TARGET_CODE) && instruction.operand.Equals(JUMP_TARGET_FIELD))
                {                    
                    Label jumpLabel = instruction.labels[0];
                    IEnumerable<CodeInstruction> injectInstructions = AssembleNewCode(jumpLabel,toInjectJump);
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
                    if (instructionList[i - 1].opcode.Equals(OpCodes.Ldloc_1) && instructionList[i + 1].opcode.Equals(OpCodes.Ldarg_0))
                    {
                        instruction.operand = toInjectJump;
                    }
                }
                yield return instruction;
            }           
        }

        private static IEnumerable<CodeInstruction> AssembleNewCode(Label outJumpLabel,Label innerJumpLabel)
        {
            //Code to inject:
            /*
             * if (Player.main.currentSub != this.subRoot)
		     * {
			 *  return;
		     * }
             * 
             */
                List<CodeInstruction> injectInstructions = new List<CodeInstruction>();

            CodeInstruction instruction = new CodeInstruction(OpCodes.Ldsfld);
            instruction.operand = typeof(Player).GetField("main", BindingFlags.Public | BindingFlags.Static);
            instruction.labels.Add(innerJumpLabel);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Callvirt);
            instruction.operand = typeof(Player).GetMethod("get_currentSub", BindingFlags.Public | BindingFlags.Instance);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Ldarg_0);            
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Ldfld);
            instruction.operand = TARGET_CLASS.GetField("subRoot", BindingFlags.Public | BindingFlags.Instance);
            injectInstructions.Add(instruction);

            instruction = new CodeInstruction(OpCodes.Call);
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
            PatchPostfix(harmony, TARGET_METHOD);
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

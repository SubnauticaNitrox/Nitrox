using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using UnityEngine;


namespace NitroxPatcher.Patches
{
    class CyclopsSonarButton_SonarPing_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSonarButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SonarPing", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly OpCode JUMP_TARGET_CODE = OpCodes.Ldsfld;
        public static readonly FieldInfo JUMP_TARGET_FIELD = typeof(SNCameraRoot).GetField("main", BindingFlags.Public | BindingFlags.Static);

        public static void Postfix(CyclopsSonarButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            //bool activeSonar = Traverse.Create(__instance).Field("sonarActive").GetValue<bool>();
            NitroxServiceLocator.LocateService<Cyclops>().SonarPing(guid);
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            //need to change the jump target for at one point
            Label toInjectJump = iLGenerator.DefineLabel();

            // find point to inject:
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

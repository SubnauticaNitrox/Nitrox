using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using System.Linq;
using NitroxClient.MonoBehaviours.Overrides;

namespace NitroxPatcher.Patches
{
    public class BaseAddBulkheadGhost_UpdatePlacement_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseAddBulkheadGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdatePlacement", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ldsfld;
        public static readonly Type OPERAND_CLASS = typeof(Player);
        public static readonly object INJECTION_OPERAND = OPERAND_CLASS.GetField("main", BindingFlags.Public | BindingFlags.Static);

        public static readonly OpCode INSTRUCTION_BEFORE_JUMP = OpCodes.Ldfld;
        public static readonly object INSTRUCTION_BEFORE_JUMP_OPERAND = typeof(SubRoot).GetField("isBase", BindingFlags.Public | BindingFlags.Instance);
        public static readonly OpCode JUMP_INSTRUCTION_TO_COPY = OpCodes.Brtrue;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            List<CodeInstruction> instructionList = instructions.ToList();

            bool shouldInject = false;

            /**
             * When placing some modules in multiplayer it throws an exception because it tries to validate
             * that the current player is in the subroot.  We want to skip over this code if we are placing 
             * a multiplayer piece:
             * 
             * if (main == null || main.currentSub == null || !main.currentSub.isBase)
             * 
             * Injected code:
             * 
             * if (!MultiplayerBuilder.isPlacing && (main == null || main.currentSub == null || !main.currentSub.isBase))
             *    
             */
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                
                if (shouldInject)
                {
                    shouldInject = false;

                    // First fetch the place we want to jump... this will be the same place as !main.currentSub.isBase
                    CodeInstruction jumpInstruction = GetJumpInstruction(instructionList, i);

                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(MultiplayerBuilder).GetMethod("get_isPlacing", BindingFlags.Public | BindingFlags.Static));
                    yield return new ValidatedCodeInstruction(OpCodes.Brtrue_S, jumpInstruction.operand); // copy the jump location
                }

                // We want to inject just after Player main = Player.main... if this is that instruction then we'll inject after the next opcode (stfld)
                shouldInject = (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND));
            }
        }

        private static CodeInstruction GetJumpInstruction(List<CodeInstruction> instructions, int startingIndex)
        {
            for (int i = startingIndex; i < instructions.Count; i++)
            {
                CodeInstruction instruction = instructions[i];

                if(instruction.opcode == INSTRUCTION_BEFORE_JUMP && instruction.operand == INSTRUCTION_BEFORE_JUMP_OPERAND)
                {
                    // we located the instruction before the jump... the next instruction should be the jump
                    CodeInstruction jmpInstruction = instructions[i + 1];

                    // Validate that it is what we are looking for
                    Validate.IsTrue(JUMP_INSTRUCTION_TO_COPY == jmpInstruction.opcode, "Looks like subnautica code has changed.  Update jump offset!");

                    return jmpInstruction;
                }
            }

            throw new Exception("Could not locate jump instruction to copy! Injection has failed.");
        }
        
        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}


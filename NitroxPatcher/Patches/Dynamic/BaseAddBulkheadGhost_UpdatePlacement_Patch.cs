using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseAddBulkheadGhost_UpdatePlacement_Patch : NitroxPatch, IDynamicPatch
    {
        /// <summary>
        ///     Unable to use <see cref="Reflect.Method" /> here because expression trees do not support out parameters (yet).
        /// </summary>
        public static readonly MethodInfo TARGET_METHOD = typeof(BaseAddBulkheadGhost).GetMethod(nameof(BaseAddBulkheadGhost.UpdatePlacement), BindingFlags.Public | BindingFlags.Instance, null,
                                                                                                 new[] { typeof(Transform), typeof(float), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(ConstructableBase) }, null);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ldsfld;
        public static readonly object INJECTION_OPERAND = Reflect.Field(() => Player.main);

        public static readonly OpCode INSTRUCTION_BEFORE_JUMP = OpCodes.Ldfld;
        public static readonly object INSTRUCTION_BEFORE_JUMP_OPERAND = Reflect.Field((SubRoot t) => t.isBase);
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
                    // First fetch the place we want to jump... this will be the same place as !main.currentSub.isBase
                    CodeInstruction jumpInstruction = GetJumpInstruction(instructionList, i);

                    yield return new CodeInstruction(OpCodes.Call, Reflect.Property(() => MultiplayerBuilder.IsPlacing).GetMethod);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, jumpInstruction.operand); // copy the jump location
                }

                // We want to inject just after Player main = Player.main... if this is that instruction then we'll inject after the next opcode (stfld)
                shouldInject = instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }

        private static CodeInstruction GetJumpInstruction(List<CodeInstruction> instructions, int startingIndex)
        {
            for (int i = startingIndex; i < instructions.Count; i++)
            {
                CodeInstruction instruction = instructions[i];

                if (instruction.opcode == INSTRUCTION_BEFORE_JUMP && instruction.operand == INSTRUCTION_BEFORE_JUMP_OPERAND)
                {
                    // we located the instruction before the jump... the next instruction should be the jump
                    CodeInstruction jmpInstruction = instructions[i + 1];

                    // Validate that it is what we are looking for
                    Validate.IsTrue(JUMP_INSTRUCTION_TO_COPY == jmpInstruction.opcode, "Looks like subnautica code has changed. Update jump offset!");

                    return jmpInstruction;
                }
            }

            throw new Exception("Could not locate jump instruction to copy! Injection has failed.");
        }
    }
}
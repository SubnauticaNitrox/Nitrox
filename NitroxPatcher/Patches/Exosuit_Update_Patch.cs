﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;
using System.Linq;
using NitroxClient.MonoBehaviours.Overrides;

namespace NitroxPatcher.Patches
{
    public class Exosuit_Update_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Exosuit);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = TARGET_CLASS.GetMethod("UpdateSounds", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                /*
                 *  When syninc exo suit we always want to skip an entire if branch in the update: 
                 * 
                 *  if(!flag)
                 *  
                 *  to do this, we transform it into something that always evaluates false:
                 *  
                 *  if(!true)
                 * 
                 */
                if (instruction.opcode == INJECTION_OPCODE && instruction.operand == INJECTION_OPERAND)
                {
                    i++; //increment to ldloc.2 (loading flag2 on evaluation stack)
                    CodeInstruction ldFlag2 = instructionList[i];

                    // Transform to if(!true)
                    ldFlag2.opcode = OpCodes.Ldc_I4_1;

                    yield return ldFlag2;
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}


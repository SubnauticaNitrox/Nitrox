using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Exosuit_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Exosuit t) => t.Update());

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = Reflect.Method((Exosuit t) => t.UpdateSounds());

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

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}


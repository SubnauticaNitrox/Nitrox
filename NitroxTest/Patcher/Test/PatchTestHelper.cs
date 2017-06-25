using Harmony;
using Harmony.ILCopying;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NitroxTest.Patcher.Test
{
    public static class PatchTestHelper
    {
        public static int GetInstructionCount(IEnumerable<CodeInstruction> result)
        {
            int instructionCounter = 0;

            foreach (CodeInstruction instruction in result)
            {
                instructionCounter++;
            }

            return instructionCounter;
        }

        public static List<CodeInstruction> GenerateDummyInstructions(int count)
        {
            List<CodeInstruction> instructions = new List<CodeInstruction>();

            for (int i = 0; i < count; i++)
            {
                instructions.Add(new CodeInstruction(OpCodes.Nop));
            }

            return instructions;
        }

        public static List<CodeInstruction> GetInstructionsFromMethod(MethodInfo targetMethod)
        {
            Validate.NotNull(targetMethod);

            List<CodeInstruction> instructions = new List<CodeInstruction>();

            foreach (ILInstruction instruction in MethodBodyReader.GetInstructions(targetMethod))
            {
                instructions.Add(instruction.GetCodeInstruction());
            }

            return instructions;
        }
    }
}

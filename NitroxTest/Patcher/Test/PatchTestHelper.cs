using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NitroxHarmony;
using NitroxHarmony.ILCopying;
using NitroxModel.Helper;

namespace NitroxTest.Patcher.Test
{
    public static class PatchTestHelper
    {
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

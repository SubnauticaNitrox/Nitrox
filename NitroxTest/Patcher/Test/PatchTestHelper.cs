using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Harmony.ILCopying;
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

        public static ReadOnlyCollection<CodeInstruction> GetInstructionsFromMethod(DynamicMethod targetMethod)
        {
            Validate.NotNull(targetMethod);
            return GetInstructionsFromIL(GetILInstructions(targetMethod));
        }
        
        public static ReadOnlyCollection<CodeInstruction> GetInstructionsFromMethod(MethodInfo targetMethod)
        {
            Validate.NotNull(targetMethod);
            return GetInstructionsFromIL(GetILInstructions(targetMethod));
        }

        private static ReadOnlyCollection<CodeInstruction> GetInstructionsFromIL(IEnumerable<ILInstruction> il)
        {
            List<CodeInstruction> result = new List<CodeInstruction>();
            foreach (ILInstruction instruction in il)
            {
                result.Add(instruction.GetCodeInstruction());
            }
            return result.AsReadOnly();
        }

        public static IEnumerable<ILInstruction> GetILInstructions(MethodInfo method)
        {
            DynamicMethod dynMethod = new DynamicMethod(method.Name, method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray(), false);
            return MethodBodyReader.GetInstructions(dynMethod.GetILGenerator(), method);
        }

        public static IEnumerable<ILInstruction> GetILInstructions(DynamicMethod method)
        {
            return MethodBodyReader.GetInstructions(method.GetILGenerator(), method);
        }
    }
}

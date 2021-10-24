using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
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

        private static ReadOnlyCollection<CodeInstruction> GetInstructionsFromIL(IEnumerable<KeyValuePair<OpCode, object>> il)
        {
            List<CodeInstruction> result = new List<CodeInstruction>();
            foreach (KeyValuePair<OpCode, object> instruction in il)
            {
                result.Add(new CodeInstruction(instruction.Key, instruction.Value));
            }
            return result.AsReadOnly();
        }

        public static IEnumerable<KeyValuePair<OpCode, object>> GetILInstructions(MethodInfo method)
        {
            DynamicMethod dynMethod = new DynamicMethod(method.Name, method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray(), false);
            return PatchProcessor.ReadMethodBody(method, dynMethod.GetILGenerator());
        }

        public static IEnumerable<KeyValuePair<OpCode, object>> GetILInstructions(DynamicMethod method)
        {
            return PatchProcessor.ReadMethodBody(method, method.GetILGenerator());
        }
    }
}

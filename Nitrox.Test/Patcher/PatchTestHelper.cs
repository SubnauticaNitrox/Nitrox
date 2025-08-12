using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;

namespace NitroxTest.Patcher
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

        public static IEnumerable<KeyValuePair<OpCode, object>> GetILInstructions(MethodInfo method)
        {
            return PatchProcessor.ReadMethodBody(method, method.GetILGenerator());
        }

        public static IEnumerable<KeyValuePair<OpCode, object>> GetILInstructions(DynamicMethod method)
        {
            return PatchProcessor.ReadMethodBody(method, method.GetILGenerator());
        }

        public static ILGenerator GetILGenerator(this MethodInfo method)
        {
            return new DynamicMethod(method.Name, method.ReturnType, method.GetParameters().Types()).GetILGenerator();
        }

        public static void TestPattern(MethodInfo targetMethod, InstructionsPattern pattern, out IEnumerable<CodeInstruction> originalIl, out IEnumerable<CodeInstruction> transformedIl)
        {
            bool shouldHappen = false;
            originalIl = PatchProcessor.GetCurrentInstructions(targetMethod);
            transformedIl = originalIl
                            .Transform(pattern, (_, _) =>
                            {
                                shouldHappen = true;
                            })
                            .ToArray(); // Required, otherwise nothing happens.

            shouldHappen.Should().BeTrue();
        }

        /// <summary>
        ///     Clones the instructions so that the returned instructions are not the same reference.
        /// </summary>
        /// <remarks>
        ///     Useful for testing code differences before and after a Harmony transpiler.
        /// </remarks>
        public static List<CodeInstruction> Clone(this IEnumerable<CodeInstruction> instructions)
        {
            return new List<CodeInstruction>(instructions.Select(il => new CodeInstruction(il)));
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
    }
}

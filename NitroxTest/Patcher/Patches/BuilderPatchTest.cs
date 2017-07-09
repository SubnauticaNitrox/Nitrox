using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using System.Reflection;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class BuilderPatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(BuilderPatch.INJECTION_OPCODE, BuilderPatch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = BuilderPatch.Transpiler(null, instructions);
            Assert.AreEqual(113, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(Builder), "TryPlace");
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(targetMethod);

            IEnumerable<CodeInstruction> result = BuilderPatch.Transpiler(targetMethod, beforeInstructions);
            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

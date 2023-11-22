using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic
{
    [TestClass]
    public class LaunchRocket_OnHandClick_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(LaunchRocket_OnHandClick_Patch.INJECTION_OPCODE, LaunchRocket_OnHandClick_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = LaunchRocket_OnHandClick_Patch.Transpiler(null, instructions);
            Assert.AreEqual(instructions.Count + 2, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(new LaunchRocket_OnHandClick_Patch().targetMethod);
            IEnumerable<CodeInstruction> result = LaunchRocket_OnHandClick_Patch.Transpiler(new LaunchRocket_OnHandClick_Patch().targetMethod, beforeInstructions);

            // We remove a lot of instructions in this transpiler
            Assert.IsTrue(beforeInstructions.Count() > result.Count());
        }
    }
}

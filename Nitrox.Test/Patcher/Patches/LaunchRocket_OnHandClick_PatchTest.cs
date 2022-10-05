using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
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
            IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(LaunchRocket_OnHandClick_Patch.TARGET_METHOD);
            IEnumerable<CodeInstruction> result = LaunchRocket_OnHandClick_Patch.Transpiler(LaunchRocket_OnHandClick_Patch.TARGET_METHOD, beforeInstructions);

            // We remove a lot of instructions in this transpiler
            Assert.IsTrue(beforeInstructions.Count() > result.Count());
        }
    }
}

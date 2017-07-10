using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using NitroxTest.Patcher.Test;
using System.Linq;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class Pickupable_Drop_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(Pickupable_Drop_Patch.INJECTION_OPCODE, Pickupable_Drop_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = Pickupable_Drop_Patch.Transpiler(null, instructions);

            Assert.AreEqual(instructions.Count + 7, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(Pickupable_Drop_Patch.TARGET_METHOD);

            IEnumerable<CodeInstruction> result = Pickupable_Drop_Patch.Transpiler(Pickupable_Drop_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
            Assert.AreEqual(beforeInstructions.Count + 7, result.Count());
        }
    }
}

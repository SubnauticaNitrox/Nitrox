using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class ConstructorInput_OnCraftingBegin_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(ConstructorInput_OnCraftingBegin_Patch.INJECTION_OPCODE, ConstructorInput_OnCraftingBegin_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = ConstructorInput_OnCraftingBegin_Patch.Transpiler(null, instructions);

            Assert.AreEqual(instructions.Count + 3, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod((DynamicMethod)ConstructorInput_OnCraftingBegin_Patch.TARGET_METHOD);
            IEnumerable<CodeInstruction> result = ConstructorInput_OnCraftingBegin_Patch.Transpiler(ConstructorInput_OnCraftingBegin_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class ConstructableBase_SetState_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(ConstructableBase_SetState_Patch.INJECTION_OPCODE, ConstructableBase_SetState_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = ConstructableBase_SetState_Patch.Transpiler(ConstructableBase_SetState_Patch.TARGET_METHOD, instructions);

            Assert.AreEqual(instructions.Count + 2, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(ConstructableBase_SetState_Patch.TARGET_METHOD);
            IEnumerable<CodeInstruction> result = ConstructableBase_SetState_Patch.Transpiler(ConstructableBase_SetState_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count() < result.Count());
        }
    }
}

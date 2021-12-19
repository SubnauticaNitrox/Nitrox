using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class CrashedShipExploder_Update_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(CrashedShipExploder_Update_Patch.INJECTION_OPCODE, CrashedShipExploder_Update_Patch.INJECTION_OPERAND));
            instructions.Add(new CodeInstruction(OpCodes.Brfalse));

            IEnumerable<CodeInstruction> result = CrashedShipExploder_Update_Patch.Transpiler(null, instructions);
            Assert.AreEqual(instructions.Count + 2, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(CrashedShipExploder_Update_Patch.TARGET_METHOD);
            IEnumerable<CodeInstruction> result = CrashedShipExploder_Update_Patch.Transpiler(CrashedShipExploder_Update_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count() <= result.Count());
        }
    }
}

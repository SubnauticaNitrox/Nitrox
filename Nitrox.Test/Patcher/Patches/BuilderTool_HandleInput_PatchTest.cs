using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class BuilderTool_HandleInput_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(BuilderTool_HandleInput_Patch.INJECTION_OPCODE, BuilderTool_HandleInput_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = BuilderTool_HandleInput_Patch.Transpiler(BuilderTool_HandleInput_Patch.TARGET_METHOD, instructions);

            Assert.AreEqual(instructions.Count + 5, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(BuilderTool_HandleInput_Patch.TARGET_METHOD);
            IEnumerable<CodeInstruction> result = BuilderTool_HandleInput_Patch.Transpiler(BuilderTool_HandleInput_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

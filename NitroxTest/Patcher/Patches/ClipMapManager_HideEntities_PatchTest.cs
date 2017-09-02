using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using NitroxTest.Patcher.Test;
using System.Linq;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class ClipMapManager_HideEntities_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(ClipMapManager_HideEntities_Patch.INJECTION_OPCODE, null));

            IEnumerable<CodeInstruction> result = ClipMapManager_HideEntities_Patch.Transpiler(null, instructions);

            Assert.AreEqual(instructions.Count + 5, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(ClipMapManager_HideEntities_Patch.TARGET_METHOD);

            IEnumerable<CodeInstruction> result = ClipMapManager_HideEntities_Patch.Transpiler(ClipMapManager_HideEntities_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
            // 3 instructions are added for every ret, currently there are two.
            Assert.AreEqual(beforeInstructions.Count + 10, result.Count());
        }
    }
}

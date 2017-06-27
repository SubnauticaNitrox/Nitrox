using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using System.Reflection.Emit;
using System.Reflection;
using Harmony.ILCopying;
using NitroxModel.Helper;
using NitroxPatcher;
using NitroxTest.Patcher.Test;

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
            Assert.AreEqual(106, PatchTestHelper.GetInstructionCount(result));
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(ClipMapManager_HideEntities_Patch.TARGET_METHOD);

            IEnumerable<CodeInstruction> result = ClipMapManager_HideEntities_Patch.Transpiler(ClipMapManager_HideEntities_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < PatchTestHelper.GetInstructionCount(result));
        }
    }
}

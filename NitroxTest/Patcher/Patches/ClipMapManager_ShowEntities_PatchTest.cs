using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class ClipMapManager_ShowEntities_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(ClipMapManager_ShowEntities_Patch.INJECTION_OPCODE, null));

            IEnumerable<CodeInstruction> result = ClipMapManager_ShowEntities_Patch.Transpiler(null, instructions);
            Assert.AreEqual(108, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(ClipMapManager_ShowEntities_Patch.TARGET_METHOD);

            IEnumerable<CodeInstruction> result = ClipMapManager_ShowEntities_Patch.Transpiler(ClipMapManager_ShowEntities_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }

        /* TODO: Figure out E-Call errors: 
        [TestMethod]
        public void HarmonySanity()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");

            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(ClipMapManager_ShowEntities_Patch.TARGET_METHOD);            
            ClipMapManager_ShowEntities_Patch.Patch(harmony);            
            List<CodeInstruction> afterInstructions = PatchTestHelper.GetInstructionsFromMethod(ClipMapManager_ShowEntities_Patch.TARGET_METHOD);

            Assert.IsTrue(beforeInstructions.Count < afterInstructions.Count);
        }*/
    }
}

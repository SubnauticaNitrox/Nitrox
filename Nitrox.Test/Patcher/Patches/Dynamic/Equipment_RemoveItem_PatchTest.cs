using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic
{
    [TestClass]
    public class Equipment_RemoveItem_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(Equipment_RemoveItem_Patch.INJECTION_OPCODE));

            IEnumerable<CodeInstruction> result = Equipment_RemoveItem_Patch.Transpiler(null, instructions);
            Assert.AreEqual(108, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(new Equipment_RemoveItem_Patch().targetMethod);
            IEnumerable<CodeInstruction> result = Equipment_RemoveItem_Patch.Transpiler(new Equipment_RemoveItem_Patch().targetMethod, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

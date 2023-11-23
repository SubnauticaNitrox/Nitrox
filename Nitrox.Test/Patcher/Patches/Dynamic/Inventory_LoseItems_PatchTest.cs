using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Inventory_LoseItems_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(Inventory_LoseItems_Patch.INJECTION_OPCODE, Inventory_LoseItems_Patch.INJECTION_OPERAND));

        IEnumerable<CodeInstruction> result = Inventory_LoseItems_Patch.Transpiler(null, instructions);
        Assert.AreEqual(instructions.Count, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(Inventory_LoseItems_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = Inventory_LoseItems_Patch.Transpiler(Inventory_LoseItems_Patch.TARGET_METHOD, beforeInstructions);

        Assert.IsTrue(beforeInstructions.Count() > result.Count());
    }
}

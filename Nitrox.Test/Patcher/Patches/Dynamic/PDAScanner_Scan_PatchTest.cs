using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class PDAScanner_Scan_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        Validate.NotNull(PDAScanner_Scan_Patch.INJECTION_OPERAND);

        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(PDAScanner_Scan_Patch.INJECTION_OPCODE, PDAScanner_Scan_Patch.INJECTION_OPERAND));

        IEnumerable<CodeInstruction> result = PDAScanner_Scan_Patch.Transpiler(new PDAScanner_Scan_Patch().targetMethod, instructions);

        Assert.AreEqual(instructions.Count + 3, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(new PDAScanner_Scan_Patch().targetMethod);
        IEnumerable<CodeInstruction> result = PDAScanner_Scan_Patch.Transpiler(new PDAScanner_Scan_Patch().targetMethod, beforeInstructions);

        Assert.AreEqual(beforeInstructions.Count() + 3, result.Count());
    }
}

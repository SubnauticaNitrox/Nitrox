using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class IngameMenu_QuitGameAsync_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(OpCodes.Call, IngameMenu_QuitGameAsync_Patch.triggerOperand));

        IEnumerable<CodeInstruction> result = IngameMenu_QuitGameAsync_Patch.Transpiler(instructions);

        Assert.AreEqual(instructions.Count, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(IngameMenu_QuitGameAsync_Patch.targetMethod);
        IEnumerable<CodeInstruction> result = IngameMenu_QuitGameAsync_Patch.Transpiler(beforeInstructions);

        Assert.IsTrue(beforeInstructions.Count() == result.Count()); // The ifs in the target method are all false in the testing environment
                                                                     // so we are left with the same code instructions.
    }
}

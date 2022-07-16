using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher.Test;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class uGUI_Pings_OnWillRenderCanvases_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(uGUI_Pings_OnWillRenderCanvases_Patch.INJECTION_OPCODE, uGUI_Pings_OnWillRenderCanvases_Patch.INJECTION_OPERAND));

        IEnumerable<CodeInstruction> result = uGUI_Pings_OnWillRenderCanvases_Patch.Transpiler(uGUI_Pings_OnWillRenderCanvases_Patch.TARGET_METHOD, instructions);
        Assert.AreEqual(instructions.Count, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(uGUI_Pings_OnWillRenderCanvases_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = uGUI_Pings_OnWillRenderCanvases_Patch.Transpiler(uGUI_Pings_OnWillRenderCanvases_Patch.TARGET_METHOD, beforeInstructions);

        Assert.IsTrue(beforeInstructions.Count == result.Count());
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class uGUI_Pings_IsVisibleNow_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        uGUI_Pings_IsVisibleNow_Patch patch = new();
        List<CodeInstruction> beforeInstructions = PatchTestHelper.GenerateDummyInstructions(100);
        beforeInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
        beforeInstructions.Add(new CodeInstruction(OpCodes.Ret));

        IEnumerable<CodeInstruction> result = uGUI_Pings_IsVisibleNow_Patch.Transpiler(patch.targetMethod, beforeInstructions.ToList());
        Assert.AreEqual(beforeInstructions.Count, result.Count());
        Assert.IsFalse(beforeInstructions.SequenceEqual(result));
    }

    [TestMethod]
    public void InjectionSanity()
    {
        uGUI_Pings_IsVisibleNow_Patch patch = new();
        ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(patch.targetMethod);
        IEnumerable<CodeInstruction> result = uGUI_Pings_IsVisibleNow_Patch.Transpiler(patch.targetMethod, beforeInstructions.ToList());

        Assert.AreEqual(beforeInstructions.Count, result.Count());
        Assert.IsFalse(beforeInstructions.SequenceEqual(result));
    }
}

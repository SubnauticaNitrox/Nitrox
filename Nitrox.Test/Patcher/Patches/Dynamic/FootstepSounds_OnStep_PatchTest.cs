using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class FootstepSounds_OnStep_PatchTest
{
    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(FootstepSounds_OnStep_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = FootstepSounds_OnStep_Patch.Transpiler(beforeInstructions);

        Assert.AreEqual(beforeInstructions.Count() + 4, result.Count());
    }
}

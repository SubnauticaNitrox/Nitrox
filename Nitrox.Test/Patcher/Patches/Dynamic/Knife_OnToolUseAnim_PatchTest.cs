using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Knife_OnToolUseAnim_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Knife_OnToolUseAnim_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Knife_OnToolUseAnim_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count());
    }
}

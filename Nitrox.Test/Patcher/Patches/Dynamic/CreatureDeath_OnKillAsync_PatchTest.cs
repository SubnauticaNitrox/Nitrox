using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class CreatureDeath_OnKillAsync_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(CreatureDeath_OnKillAsync_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = CreatureDeath_OnKillAsync_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 9);
    }
}

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Flare_Update_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Flare_Update_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Flare_Update_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count());
    }
}

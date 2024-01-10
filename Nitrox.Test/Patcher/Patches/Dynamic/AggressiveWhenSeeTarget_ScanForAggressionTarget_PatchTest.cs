using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class AggressiveWhenSeeTarget_ScanForAggressionTarget_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(AggressiveWhenSeeTarget_ScanForAggressionTarget_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = AggressiveWhenSeeTarget_ScanForAggressionTarget_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

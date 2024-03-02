using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Respawn_Start_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Respawn_Start_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Respawn_Start_Patch.Transpiler(Respawn_Start_Patch.TARGET_METHOD, originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

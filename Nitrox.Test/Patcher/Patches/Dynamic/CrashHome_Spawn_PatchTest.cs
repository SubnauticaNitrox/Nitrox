using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class CrashHome_Spawn_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(CrashHome_Spawn_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = CrashHome_Spawn_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 2);
    }
}

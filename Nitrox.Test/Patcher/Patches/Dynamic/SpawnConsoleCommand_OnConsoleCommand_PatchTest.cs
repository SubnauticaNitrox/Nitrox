using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class SpawnConsoleCommand_OnConsoleCommand_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(SpawnConsoleCommand_OnConsoleCommand_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = SpawnConsoleCommand_OnConsoleCommand_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 2);
    }
}

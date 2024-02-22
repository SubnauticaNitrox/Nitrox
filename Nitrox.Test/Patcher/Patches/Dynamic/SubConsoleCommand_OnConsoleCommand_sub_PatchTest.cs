using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class SubConsoleCommand_OnConsoleCommand_sub_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(SubConsoleCommand_OnConsoleCommand_sub_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = SubConsoleCommand_OnConsoleCommand_sub_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count());
    }
}

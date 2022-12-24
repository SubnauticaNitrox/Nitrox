using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher;

namespace Nitrox.Test.Patcher.Patches.Dynamic;

[TestClass]
public class DevConsole_Update_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        PatchTestHelper.TestPattern(DevConsole_Update_Patch.TargetMethod, DevConsole_Update_Patch.DevConsoleSetStateTruePattern, out IEnumerable<CodeInstruction> originalIl, out IEnumerable<CodeInstruction> transformedIl);
        originalIl.Count().Should().Be(transformedIl.Count());
    }
}

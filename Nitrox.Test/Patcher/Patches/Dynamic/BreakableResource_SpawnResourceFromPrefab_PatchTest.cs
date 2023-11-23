using FluentAssertions;
using HarmonyLib;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static NitroxPatcher.Patches.Dynamic.BreakableResource_SpawnResourceFromPrefab_Patch;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class BreakableResource_SpawnResourceFromPrefab_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        ReadOnlyCollection<CodeInstruction> originalIL = PatchTestHelper.GetInstructionsFromMethod(TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIL = Transpiler(TARGET_METHOD, originalIL);
        originalIL.Count.Should().Be(transformedIL.Count() - 2);
    }
}

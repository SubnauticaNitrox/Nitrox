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
        BreakableResource_SpawnResourceFromPrefab_Patch patch = new();
        ReadOnlyCollection<CodeInstruction> originalIL = PatchTestHelper.GetInstructionsFromMethod(patch.targetMethod);
        IEnumerable<CodeInstruction> transformedIL = Transpiler(patch.targetMethod, originalIL);
        originalIL.Count.Should().Be(transformedIL.Count() - 2);
    }
}

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class CreatureDeath_SpawnRespawner_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(CreatureDeath_SpawnRespawner_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = CreatureDeath_SpawnRespawner_Patch.Transpiler(CreatureDeath_SpawnRespawner_Patch.TARGET_METHOD, originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 2);
    }
}

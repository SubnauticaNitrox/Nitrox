using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class uSkyManager_SetVaryingMaterialProperties_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        PatchTestHelper.TestPattern(uSkyManager_SetVaryingMaterialProperties_Patch.TARGET_METHOD, uSkyManager_SetVaryingMaterialProperties_Patch.ModifyInstructionPattern, out IEnumerable<CodeInstruction> originalIl, out IEnumerable<CodeInstruction> transformedIl);
        originalIl.Count().Should().Be(transformedIl.Count());
    }
}

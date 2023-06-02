using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using static NitroxPatcher.Patches.Dynamic.uSkyManager_SetVaryingMaterialProperties_Patch;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class uSkyManager_SetVaryingMaterialProperties_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        ReadOnlyCollection<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(TARGET_METHOD);
        CodeInstruction[] transformedIl = Transpiler(TARGET_METHOD, originalIl.Clone()).ToArray();
        originalIl.Count.Should().Be(transformedIl.Length);
        originalIl.Should().NotBeEquivalentTo(transformedIl);
    }
}

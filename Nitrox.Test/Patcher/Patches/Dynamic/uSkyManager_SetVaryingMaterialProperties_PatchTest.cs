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
        uSkyManager_SetVaryingMaterialProperties_Patch patch = new uSkyManager_SetVaryingMaterialProperties_Patch();

        ReadOnlyCollection<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(patch.targetMethod);
        CodeInstruction[] transformedIl = Transpiler(patch.targetMethod, originalIl.Clone()).ToArray();
        originalIl.Count.Should().Be(transformedIl.Length);
        originalIl.Should().NotBeEquivalentTo(transformedIl);
    }
}

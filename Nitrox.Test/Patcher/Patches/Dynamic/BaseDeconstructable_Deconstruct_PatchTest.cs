using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class BaseDeconstructable_Deconstruct_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(BaseDeconstructable_Deconstruct_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = BaseDeconstructable_Deconstruct_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - BaseDeconstructable_Deconstruct_Patch.InstructionsToAdd(true).Count() * 2);
    }
}

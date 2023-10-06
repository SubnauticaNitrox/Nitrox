using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using System.Collections.Generic;
using System.Linq;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Builder_TryPlace_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Builder_TryPlace_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Builder_TryPlace_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - (Builder_TryPlace_Patch.InstructionsToAdd1.Count + Builder_TryPlace_Patch.InstructionsToAdd2.Count));
    }
}

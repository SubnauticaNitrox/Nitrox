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
        
        Builder_TryPlace_Patch patch = new();
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(patch.targetMethod);
        IEnumerable<CodeInstruction> transformedIl = Builder_TryPlace_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - (Builder_TryPlace_Patch.InstructionsToAdd1.Count + Builder_TryPlace_Patch.InstructionsToAdd2.Count));
    }
}

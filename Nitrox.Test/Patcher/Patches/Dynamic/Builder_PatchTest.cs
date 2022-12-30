using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher;

namespace Nitrox.Test.Patcher.Patches.Dynamic;

[TestClass]
public class Builder_PatchTest
{
    [TestMethod]
    public void SanityTryplace()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Builder_Patch.TARGET_METHOD_TRYPLACE);
        IEnumerable<CodeInstruction> transformedIl = Builder_Patch.TranspilerTryplace(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - (Builder_Patch.InstructionsToAdd1.Count + Builder_Patch.InstructionsToAdd2.Count));
    }

    [TestMethod]
    public void SanityConstruct()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Builder_Patch.TARGET_METHOD_CONSTRUCT);
        IEnumerable<CodeInstruction> transformedIl = Builder_Patch.TranspilerConstruct(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - Builder_Patch.ConstructionInstructionsToAdd.Count);
    }

    [TestMethod]
    public void SanityDeconstruct()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Builder_Patch.TARGET_METHOD_DECONSTRUCT_ASYNC);
        IEnumerable<CodeInstruction> transformedIl = Builder_Patch.TranspilerDeconstructAsync(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - Builder_Patch.DeconstructionInstructionsToAdd.Count);
    }

    [TestMethod]
    public void SanityBaseDeconstruct()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Builder_Patch.TARGET_METHOD_DECONSTRUCT);
        IEnumerable<CodeInstruction> transformedIl = Builder_Patch.TranspilerBaseDeconstruct(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - Builder_Patch.MakeBaseDeconstructInstructionsToAdd(true).Count() * 2);
    }
}

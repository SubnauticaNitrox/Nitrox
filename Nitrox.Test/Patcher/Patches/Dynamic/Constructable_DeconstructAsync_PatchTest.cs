using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using System.Collections.Generic;
using System.Linq;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Constructable_DeconstructAsync_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(new Constructable_DeconstructAsync_Patch().targetMethod);
        IEnumerable<CodeInstruction> transformedIl = Constructable_DeconstructAsync_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - Constructable_DeconstructAsync_Patch.InstructionsToAdd.Count);
    }
}

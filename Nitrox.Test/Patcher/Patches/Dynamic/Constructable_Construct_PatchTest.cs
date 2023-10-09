using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using System.Collections.Generic;
using System.Linq;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Constructable_Construct_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Constructable_Construct_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Constructable_Construct_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - Constructable_Construct_Patch.InstructionsToAdd.Count);
    }
}

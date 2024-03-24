using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Vehicle_TorpedoShot_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Vehicle_TorpedoShot_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Vehicle_TorpedoShot_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

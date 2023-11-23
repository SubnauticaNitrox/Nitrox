using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class ConstructableBase_SetState_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(ConstructableBase_SetState_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = ConstructableBase_SetState_Patch.Transpiler(null, originalIl);
        originalIl.Count().Should().Be(transformedIl.Count() - ConstructableBase_SetState_Patch.InstructionsToAdd.Count);
    }
}

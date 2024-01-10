using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class AttackCyclops_OnCollisionEnter_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(AttackCyclops_OnCollisionEnter_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = AttackCyclops_OnCollisionEnter_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() - 17);
        
    }
}

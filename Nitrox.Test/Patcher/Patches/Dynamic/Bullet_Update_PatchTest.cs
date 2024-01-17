using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Bullet_Update_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        ILGenerator generator = Bullet_Update_Patch.TARGET_METHOD.GetILGenerator();
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Bullet_Update_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Bullet_Update_Patch.Transpiler(originalIl, generator);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

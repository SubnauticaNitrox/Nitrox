using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class AttackCyclops_UpdateAggression_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(AttackCyclops_UpdateAggression_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = AttackCyclops_UpdateAggression_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() - 23);
    }
}

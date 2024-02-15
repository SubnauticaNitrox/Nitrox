using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class SeaDragonMeleeAttack_OnTouchFront_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(SeaDragonMeleeAttack_OnTouchFront_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = SeaDragonMeleeAttack_OnTouchFront_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 9);
    }
}

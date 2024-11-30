using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class SeaDragonMeleeAttack_SwatAttack_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(SeaDragonMeleeAttack_SwatAttack_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = SeaDragonMeleeAttack_SwatAttack_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 4);
    }
}

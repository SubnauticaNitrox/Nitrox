using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Poop_Perform_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Poop_Perform_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Poop_Perform_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 1);
    }
}

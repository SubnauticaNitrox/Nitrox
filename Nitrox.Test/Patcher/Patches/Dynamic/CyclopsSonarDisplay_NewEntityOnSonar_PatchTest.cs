using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class CyclopsSonarDisplay_NewEntityOnSonar_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(CyclopsSonarDisplay_NewEntityOnSonar_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = CyclopsSonarDisplay_NewEntityOnSonar_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

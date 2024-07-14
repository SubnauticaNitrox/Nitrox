using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class GrowingPlant_SpawnGrownModelAsync_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(GrowingPlant_SpawnGrownModelAsync_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = GrowingPlant_SpawnGrownModelAsync_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() - 1);
    }
}

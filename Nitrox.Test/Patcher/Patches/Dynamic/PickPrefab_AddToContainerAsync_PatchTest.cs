using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class PickPrefab_AddToContainerAsync_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(PickPrefab_AddToContainerAsync_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = PickPrefab_AddToContainerAsync_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 4);
    }
}

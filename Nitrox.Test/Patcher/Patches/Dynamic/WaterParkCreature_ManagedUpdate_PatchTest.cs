using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class WaterParkCreature_ManagedUpdate_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(WaterParkCreature_ManagedUpdate_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = WaterParkCreature_ManagedUpdate_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 2);
    }
}

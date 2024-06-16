using HarmonyLib;
using NitroxPatcher.PatternMatching;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class Inventory_LoseItems_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(Inventory_LoseItems_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = Inventory_LoseItems_Patch.Transpiler(originalIl);
        Console.WriteLine(transformedIl.ToPrettyString());
        transformedIl.Count().Should().Be(originalIl.Count() + 5);
    }
}

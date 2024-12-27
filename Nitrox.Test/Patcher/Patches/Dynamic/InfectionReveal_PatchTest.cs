using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;
[TestClass]
public class InfectionReveal_PatchTest
{
    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(InfectionReveal_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = InfectionReveal_Patch.Transpiler(beforeInstructions);
        Assert.AreEqual(beforeInstructions.Count() + 1, result.Count());
    }
}

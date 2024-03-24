using HarmonyLib;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class SeaTreaderSounds_SpawnChunks_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        IEnumerable<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(SeaTreaderSounds_SpawnChunks_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> transformedIl = SeaTreaderSounds_SpawnChunks_Patch.Transpiler(originalIl);
        transformedIl.Count().Should().Be(originalIl.Count() + 3);
    }
}

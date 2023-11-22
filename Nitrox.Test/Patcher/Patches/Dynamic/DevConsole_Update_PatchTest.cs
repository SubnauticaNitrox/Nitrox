using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using static NitroxPatcher.Patches.Dynamic.DevConsole_Update_Patch;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class DevConsole_Update_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        ReadOnlyCollection<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(new DevConsole_Update_Patch().targetMethod);
        CodeInstruction[] transformedIl = Transpiler(new DevConsole_Update_Patch().targetMethod, originalIl.Clone()).ToArray();
        originalIl.Count.Should().Be(transformedIl.Length, "the modified code shouldn't have a difference in size");
        transformedIl.Should().NotBeEquivalentTo(originalIl, "the patch should have changed the IL");
    }
}

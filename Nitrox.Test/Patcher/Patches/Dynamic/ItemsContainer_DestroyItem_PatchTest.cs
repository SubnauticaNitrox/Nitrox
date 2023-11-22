using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxTest.Patcher;
using static NitroxPatcher.Patches.Dynamic.ItemsContainer_DestroyItem_Patch;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class ItemsContainer_DestroyItem_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        ReadOnlyCollection<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(new ItemsContainer_DestroyItem_Patch().targetMethod) ;
        IEnumerable<CodeInstruction> transformedIl = Transpiler(new ItemsContainer_DestroyItem_Patch().targetMethod, originalIl);
        originalIl.Count.Should().Be(transformedIl.Count() - 2);
    }
}

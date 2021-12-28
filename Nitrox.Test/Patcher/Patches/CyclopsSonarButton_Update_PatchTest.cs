using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
using NitroxTest.Patcher.Test;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class CyclopsSonarButton_Update_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(CyclopsSonarButton_Update_Patch.INJECTION_OPCODE, CyclopsSonarButton_Update_Patch.INJECTION_OPERAND));
        instructions.Add(new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Player player) => player.GetMode())));
        instructions.Add(new CodeInstruction(OpCodes.Brtrue));

        IEnumerable<CodeInstruction> result = CyclopsSonarButton_Update_Patch.Transpiler(null, instructions);
        Assert.AreEqual(instructions.Count + 2, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(CyclopsSonarButton_Update_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = CyclopsSonarButton_Update_Patch.Transpiler(CyclopsSonarButton_Update_Patch.TARGET_METHOD, beforeInstructions);

        Assert.IsTrue(beforeInstructions.Count < result.Count());
    }

    [TestMethod]
    public void CheckMethodValidity()
    {
        ReadOnlyCollection<CodeInstruction> instructions = PatchTestHelper.GetInstructionsFromMethod(CyclopsSonarButton_Update_Patch.TARGET_METHOD);
        for (int i = 0; i < instructions.Count; i++)
        {
            CodeInstruction instruction = instructions[i];
            if (instruction.opcode.Equals(CyclopsSonarButton_Update_Patch.INJECTION_OPCODE) && instruction.operand.Equals(CyclopsSonarButton_Update_Patch.INJECTION_OPERAND))
            {
                CodeInstruction nextBr = instructions[i + 2];
                Assert.IsTrue(nextBr.opcode.Equals(OpCodes.Brtrue), "Looks like subnautica code has changed. Update jump offset!");
            }
        }
    }
}

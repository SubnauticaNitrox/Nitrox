using System.Collections.ObjectModel;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher;

namespace Nitrox.Test.Patcher.Patches.Dynamic;

[TestClass]
public class CyclopsSonarButton_Update_PatchTest
{
    [TestMethod]
    public void CheckJumpOffsetValidity()
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

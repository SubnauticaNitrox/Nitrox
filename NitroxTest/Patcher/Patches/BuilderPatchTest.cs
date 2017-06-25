using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using System.Reflection.Emit;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class BuilderPatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = new List<CodeInstruction>();

            for(int i = 0; i < BuilderPatch.EXPECTED_INJECT_POINT; i++)
            {
                instructions.Add(new CodeInstruction(OpCodes.Nop));
            }

            instructions.Add(new CodeInstruction(OpCodes.Call, BuilderPatch.getRequiredOperand()));

            IEnumerable<CodeInstruction> result = BuilderPatch.Transpiler(null, instructions);

            int instructionCounter = 0;

            foreach(CodeInstruction instruction in result)
            {
                instructionCounter++;
            }
            
            Assert.AreEqual(145, instructionCounter);
        }
    }
}

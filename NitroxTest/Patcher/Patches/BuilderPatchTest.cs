using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using System.Reflection.Emit;
using System.Reflection;
using Harmony.ILCopying;
using NitroxModel.Helper;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class BuilderPatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = GenerateDummyInstructions();
            instructions.Add(new CodeInstruction(BuilderPatch.INJECTION_OPCODE, BuilderPatch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = BuilderPatch.Transpiler(null, instructions);
            Assert.AreEqual(113, getInstructionCount(result));
        }

        [TestMethod]
        public void InjectionSanity()
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(Builder), "TryPlace");
            List<CodeInstruction> beforeInstructions = new List<CodeInstruction>();

            foreach(ILInstruction instruction in MethodBodyReader.GetInstructions(targetMethod))
            {
                beforeInstructions.Add(instruction.GetCodeInstruction());
            }

            IEnumerable<CodeInstruction> result = BuilderPatch.Transpiler(targetMethod, beforeInstructions);
            Assert.IsTrue(beforeInstructions.Count < getInstructionCount(result));
        }

        private int getInstructionCount(IEnumerable<CodeInstruction> result)
        {
            int instructionCounter = 0;

            foreach (CodeInstruction instruction in result)
            {
                instructionCounter++;
            }

            return instructionCounter;
        }

        private List<CodeInstruction> GenerateDummyInstructions()
        {
            List<CodeInstruction> instructions = new List<CodeInstruction>();

            for (int i = 0; i < 100; i++)
            {
                instructions.Add(new CodeInstruction(OpCodes.Nop));
            }

            return instructions;
        }
    }
}

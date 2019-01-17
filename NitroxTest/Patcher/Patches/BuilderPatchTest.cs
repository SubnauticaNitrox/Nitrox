using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using NitroxTest.Patcher.Test;
using System.Reflection.Emit;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class BuilderPatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(Builder_TryPlace_Patch.PLACE_BASE_INJECTION_OPCODE, Builder_TryPlace_Patch.PLACE_BASE_INJECTION_OPERAND));
            instructions.Add(new CodeInstruction(Builder_TryPlace_Patch.PLACE_FURNITURE_INJECTION_OPCODE, Builder_TryPlace_Patch.PLACE_FURNITURE_INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = Builder_TryPlace_Patch.Transpiler(null, instructions);
            Assert.AreEqual(121, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            DynamicMethod targetMethod = (DynamicMethod)AccessTools.Method(typeof(Builder), "TryPlace");
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(targetMethod);

            IEnumerable<CodeInstruction> result = Builder_TryPlace_Patch.Transpiler(targetMethod, beforeInstructions);
            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

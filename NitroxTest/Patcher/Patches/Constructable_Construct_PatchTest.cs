using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using Harmony;
using System.Reflection;
using NitroxTest.Patcher.Test;
using System.Linq;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class Constructable_Construct_PatchTest
    {
        // Constructable_Construct_Patch is currently commented out, so the unittests don't work.
        //[TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(Constructable_Construct_Patch.INJECTION_OPCODE, Constructable_Construct_Patch.INJECTION_OPERAND));

            IEnumerable<CodeInstruction> result = Constructable_Construct_Patch.Transpiler(null, instructions);
            Assert.AreEqual(111, result.Count());
        }

        //[TestMethod]
        public void InjectionSanity()
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(Constructable), "Construct");
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(targetMethod);

            IEnumerable<CodeInstruction> result = Constructable_Construct_Patch.Transpiler(targetMethod, beforeInstructions);
            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

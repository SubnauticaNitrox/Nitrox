using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxPatcher.Patches;
using NitroxTest.Patcher.Test;

namespace NitroxTest.Patcher.Patches
{
    [TestClass]
    public class Equipment_RemoveItem_PatchTest
    {
        [TestMethod]
        public void Sanity()
        {
            List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
            instructions.Add(new CodeInstruction(Equipment_RemoveItem_Patch.INJECTION_OPCODE));
            MethodInfo method = typeof(BaseGhost).GetMethod("RemoveItem", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(bool), typeof(bool) }, null);
            IEnumerable<CodeInstruction> result = Equipment_RemoveItem_Patch.Transpiler(method, instructions);
            Assert.AreEqual(108, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            List<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(Equipment_RemoveItem_Patch.TARGET_METHOD);

            IEnumerable<CodeInstruction> result = Equipment_RemoveItem_Patch.Transpiler(Equipment_RemoveItem_Patch.TARGET_METHOD, beforeInstructions);

            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

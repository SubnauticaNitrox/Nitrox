using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
using NitroxPatcher.Patches.Dynamic;
using NitroxTest.Patcher.Test;

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
            Assert.AreEqual(120, result.Count());
        }

        [TestMethod]
        public void InjectionSanity()
        {
            MethodInfo targetMethod = Reflect.Method(() => Builder.TryPlace());
            ReadOnlyCollection<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(targetMethod);

            IEnumerable<CodeInstruction> result = Builder_TryPlace_Patch.Transpiler(targetMethod, beforeInstructions);
            Assert.IsTrue(beforeInstructions.Count < result.Count());
        }
    }
}

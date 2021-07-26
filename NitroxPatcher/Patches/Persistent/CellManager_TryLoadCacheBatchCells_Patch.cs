using System;
using System.Reflection;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace NitroxPatcher.Patches.Persistent
{
    class CellManager_TryLoadCacheBatchCells_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryLoadCacheBatchCells", BindingFlags.Public | BindingFlags.Instance);
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instrList = instructions.ToList();
            for (int i = 0; i < instrList.Count; i++)
            {
                CodeInstruction instruction = instrList[i];
                if (instrList.Count > i + 2 && instrList[i+2].opcode == OpCodes.Callvirt && instrList[i+2].operand == (object)typeof(LargeWorldStreamer).GetProperty("pathPrefix", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, ""); // Replace pathPrefix with an empty string
                    i += 2;
                }
                else if (instrList.Count > i + 2 && instrList[i + 2].opcode == OpCodes.Callvirt && instrList[i + 2].operand == (object)typeof(LargeWorldStreamer).GetProperty("fallbackPrefix", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, ""); // Replace fallbackPrefix with an empty string
                    i += 2; // Now that I think of it this is skipping the entire call?
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

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
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> instrList = instructions.ToList();

            Label pathPrefixJmp = generator.DefineLabel();
            
            Label labeledPathInstructionJmp = generator.DefineLabel();
            
            Label fallbackPrefixJmp = generator.DefineLabel();

            Label labeledFallbackInstructionJmp = generator.DefineLabel();

            for (int i = 0; i < instrList.Count; i++)
            {
                CodeInstruction instruction = instrList[i];
                if (instrList.Count > i + 2 && instrList[i+2].opcode == OpCodes.Callvirt && instrList[i+2].operand == (object)typeof(LargeWorldStreamer).GetProperty("pathPrefix", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                {
                    foreach (CodeInstruction instr in TranspilerHelper.IsMultiplayer(pathPrefixJmp, generator))
                    {
                        yield return instr;
                    }

                    yield return new CodeInstruction(OpCodes.Ldstr, ""); // Replace pathPrefix with an empty string
                    yield return new CodeInstruction(OpCodes.Br, labeledPathInstructionJmp);
                    instrList[i].labels.Add(pathPrefixJmp);
                    yield return instrList[i];
                    yield return instrList[i + 1];
                    yield return instrList[i + 2];

                    CodeInstruction labeledCodeInstruction = new CodeInstruction(instrList[i + 3].opcode, instrList[i + 3].operand);
                    labeledCodeInstruction.labels.Add(labeledPathInstructionJmp);

                    yield return labeledCodeInstruction;
                    i += 3;
                }
                else if (instrList.Count > i + 2 && instrList[i + 2].opcode == OpCodes.Callvirt && instrList[i + 2].operand == (object)typeof(LargeWorldStreamer).GetProperty("fallbackPrefix", BindingFlags.Public | BindingFlags.Instance).GetGetMethod())
                {
                    foreach (CodeInstruction instr in TranspilerHelper.IsMultiplayer(fallbackPrefixJmp, generator))
                    {
                        yield return instr;
                    }

                    yield return new CodeInstruction(OpCodes.Ldstr, ""); // Replace pathPrefix with an empty string
                    yield return new CodeInstruction(OpCodes.Br, labeledFallbackInstructionJmp);
                    instrList[i].labels.Add(fallbackPrefixJmp);
                    yield return instrList[i];
                    yield return instrList[i + 1];
                    yield return instrList[i + 2];

                    CodeInstruction labeledCodeInstruction = new CodeInstruction(instrList[i + 3].opcode, instrList[i + 3].operand);
                    labeledCodeInstruction.labels.Add(labeledFallbackInstructionJmp);

                    yield return labeledCodeInstruction;
                    i += 3;
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

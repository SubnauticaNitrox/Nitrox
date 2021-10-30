using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    class CellManager_TryLoadCacheBatchCells_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CellManager t) => t.TryLoadCacheBatchCells(default(BatchCells)));
        private static readonly MethodInfo FALLBACK_PREFIX_GETTER = Reflect.Property((LargeWorldStreamer t) => t.fallbackPrefix).GetMethod;
        private static readonly MethodInfo PATH_PREFIX_GETTER = Reflect.Property((LargeWorldStreamer t) => t.pathPrefix).GetMethod;

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
                if (instrList.Count > i + 2 && instrList[i + 2].opcode == OpCodes.Callvirt && ReferenceEquals(instrList[i + 2].operand, PATH_PREFIX_GETTER))
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

                    CodeInstruction labeledCodeInstruction = new(instrList[i + 3].opcode, instrList[i + 3].operand);
                    labeledCodeInstruction.labels.Add(labeledPathInstructionJmp);

                    yield return labeledCodeInstruction;
                    i += 3;
                }
                else
                {
                    
                    if (instrList.Count > i + 2 && instrList[i + 2].opcode == OpCodes.Callvirt && ReferenceEquals(instrList[i + 2].operand, FALLBACK_PREFIX_GETTER))
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

                        CodeInstruction labeledCodeInstruction = new(instrList[i + 3].opcode, instrList[i + 3].operand);
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
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

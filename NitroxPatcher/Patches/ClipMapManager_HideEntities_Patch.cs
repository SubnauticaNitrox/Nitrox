using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NitroxPatcher.Patches
{
    public class ClipMapManager_HideEntities_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ClipMapManager).GetNestedType("Cell", BindingFlags.NonPublic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HideEntities");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ret;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * Multiplayer.RemoveChunk(this.chunk);
                     */

                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_chunk"));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod("RemoveChunk", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(VoxelandChunk) }, null));
                }

                yield return instruction;
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

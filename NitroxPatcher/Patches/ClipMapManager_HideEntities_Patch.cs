using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class ClipMapManager_HideEntities_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ClipMapManager).GetNestedType("Cell", BindingFlags.NonPublic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HideEntities");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ret;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var chunk = generator.DeclareLocal(typeof(VoxelandChunk));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * VoxelandChunk chunk = this.chunk;
                     * if (chunk != null)
                     *     Multiplayer.RemoveChunk(chunk.transform.position);
                     */

                    // One of the two ret's has a label, reuse it to make the Harmony log more concise.
                    if (instruction.labels.Count == 0)
                        instruction.labels.Add(generator.DefineLabel());
                    Label skipNull = instruction.labels[0];

                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_chunk"));
                    yield return new ValidatedCodeInstruction(OpCodes.Stloc, chunk);

                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc, chunk);
                    // Harmony converts Brfalse_S to Brfalse (because it doesn't handle short jumps properly if I understand their comment correctly), so just use Brfalse here for clarity.
                    yield return new ValidatedCodeInstruction(OpCodes.Brfalse, skipNull);

                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc, chunk);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Component).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod("RemoveChunk", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Vector3) }, null));
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

using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

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
                     * Multiplayer.RemoveChunk(this.chunk.transform.position);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_chunk"));
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


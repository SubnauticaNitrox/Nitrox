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
    public class ClipMapManager_ShowEntities_Patch
    {
        public static readonly Type TARGET_CLASS = typeof(ClipMapManager).GetNestedType("Cell", BindingFlags.NonPublic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ShowEntities");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ret;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * Multiplayer.AddChunk(this.chunk.transform.position, this.mgr);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_chunk"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(GameObject).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_mgr"));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Multiplayer).GetMethod("AddChunk", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Vector3), typeof(MonoBehaviour) }, null));
                }
            }
        }

        public static void Patch(HarmonyInstance harmony)
        {
            MethodInfo transpiler = typeof(ClipMapManager_ShowEntities_Patch).GetMethod("Transpiler");
            Validate.NotNull(transpiler);
            HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(ClipMapManager_ShowEntities_Patch), "Transpiler");
            harmony.Patch(TARGET_METHOD, null, null, harmonyMethod);
        }
    }
}


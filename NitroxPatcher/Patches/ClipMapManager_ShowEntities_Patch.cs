using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class ClipMapManager_ShowEntities_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ClipMapManager).GetNestedType("Cell", BindingFlags.NonPublic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ShowEntities");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Ret;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * Multiplayer.AddChunk(this.chunk.transform.position, this.mgr);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Chunks", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_chunk"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Component).GetMethod("get_transform"));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Transform).GetMethod("get_position"));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, TARGET_CLASS.GetMethod("get_mgr"));
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(Chunks).GetMethod("AddChunk"));
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


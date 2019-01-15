using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches
{
    public class BaseDeconstructable_Deconstruct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = typeof(Constructable).GetMethod("SetState");

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * BaseDeconstructable_Deconstruct_Patch.Callback(gameObject);
                     */
                    yield return original.Ldloc<GameObject>(1);
                    yield return new CodeInstruction(OpCodes.Call, typeof(BaseDeconstructable_Deconstruct_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }
            }
        }

        public static void Callback(GameObject gameObject)
        {
            TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE, gameObject);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}


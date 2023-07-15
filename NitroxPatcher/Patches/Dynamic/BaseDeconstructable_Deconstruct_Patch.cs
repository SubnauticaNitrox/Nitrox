using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public sealed partial class BaseDeconstructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseDeconstructable t) => t.Deconstruct());

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = Reflect.Method((Constructable t) => t.SetState(default(bool), default(bool)));

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
                    yield return original.Ldloc<ConstructableBase>(0);
                    yield return new CodeInstruction(OpCodes.Call, ((Action<GameObject>)Callback).Method);
                }
            }
        }

        public static void Callback(GameObject gameObject)
        {
            TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GHOST, gameObject);
        }
    }
}

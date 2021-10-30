using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseGhost_Finish_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseGhost t) => t.Finish());

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
        public static readonly object INJECTION_OPERAND = Reflect.Field((BaseGhost t) => t.targetBase);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    /*
                     * TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT, gameObject);
                     */
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return original.Ldloc<GameObject>(1);
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Add(default(TransientObjectType), default(object))));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}


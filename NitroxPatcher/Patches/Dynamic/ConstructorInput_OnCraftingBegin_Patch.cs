using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructorInput_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ConstructorInput t) => t.OnCraftingBegin(default(TechType), default(float)));

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = Reflect.Method((Constructor t) => t.SendBuildBots(default(GameObject)));

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT, gameObject);
                     */
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return original.Ldloc<GameObject>(0);
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


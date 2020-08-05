using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructorInput_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ConstructorInput);
        public static readonly MethodInfo TARGET_METHOD = GetMethod();

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = typeof(Constructor).GetMethod("SendBuildBots", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(GameObject) }, null);

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
                    yield return new CodeInstruction(OpCodes.Call, typeof(TransientLocalObjectManager).GetMethod("Add", BindingFlags.Static | BindingFlags.Public, null, new Type[] { TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT.GetType(), typeof(object) }, null));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }

        private static Type GetLoadAsyncEnumerableMethod()
        {
            Type[] nestedTypes = TARGET_CLASS.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static);
            Type targetEnumeratorClass = null;

            foreach (Type type in nestedTypes)
            {
                if (type.FullName?.Contains("OnCraftingBeginAsync") == true)
                {
                    targetEnumeratorClass = type;
                }
            }

            Validate.NotNull(targetEnumeratorClass);
            return targetEnumeratorClass;
        }

        private static MethodInfo GetMethod()
        {
            MethodInfo method = GetLoadAsyncEnumerableMethod().GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(method);

            return method;
        }
    }
}


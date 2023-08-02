using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public sealed partial class ConstructorInput_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((ConstructorInput t) => t.OnCraftingBeginAsync(default(TechType), default(float))));

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = Reflect.Method(() => CrafterLogic.NotifyCraftEnd(default(GameObject), default(TechType)));

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * Callback(constructor, gameObject, techType, duration);
                     */
                    yield return original.Ldloc<ConstructorInput>(0);
                    yield return original.Ldloc<GameObject>(0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, TARGET_METHOD.DeclaringType.GetField("techType", BindingFlags.Instance | BindingFlags.Public));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, TARGET_METHOD.DeclaringType.GetField("duration", BindingFlags.Instance | BindingFlags.Public));
                    yield return new CodeInstruction(OpCodes.Call, ((Action<ConstructorInput, GameObject, TechType, float>)Callback).Method);
                }
            }
        }

        public static void Callback(ConstructorInput constructor, GameObject constructedObject, TechType techType, float duration)
        {
            Resolve<MobileVehicleBay>().BeginCrafting(constructor, constructedObject, techType, duration);
        }
    }
}

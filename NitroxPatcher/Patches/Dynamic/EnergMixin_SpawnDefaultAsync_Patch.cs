using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When items are spawned in Subnautica will automatically add batteries to them async.  This is challenging to deal with because it is 
/// difficult to discriminate between these async/default additions and a user purposfully placing batteries into an object. Instead, we
/// disable the default behavior and allow Nitrox to always spawn the batteries.  This guarentees we can capture the ids correctly.
/// </summary>
public class EnergyMixin_SpawnDefaultAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_ORIGINAL = Reflect.Method((EnergyMixin t) => t.SpawnDefaultAsync(default(float), default(TaskResult<bool>)));
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ORIGINAL);

    public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
    public static readonly object INJECTION_OPERAND = Reflect.Method(() => CrafterLogic.NotifyCraftEnd(default(GameObject), default(TechType)));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);

        // Blank out the method and replace with:
        //
        //      result.set(false);
        //      return false;
        //
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Ldfld, TARGET_METHOD.DeclaringType.GetField("result", BindingFlags.Instance | BindingFlags.Public));
        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
        yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((TaskResult<bool> result) => result.Set(default)));
        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}


using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ensures the in-game log is animated smoothly regardless of the time scale.
/// </summary>
public sealed class ErrorMessage_OnLateUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_ON_LATE_UPDATE = Reflect.Method((ErrorMessage t) => t.OnLateUpdate());
    private static readonly MethodInfo TARGET_METHOD_ADD_MESSAGE = Reflect.Method((ErrorMessage t) => t._AddMessage(default));
    private static readonly MethodInfo TARGET_OPERAND_TIME = Reflect.Property(() => PDA.time).GetMethod;
    private static readonly MethodInfo TARGET_OPERAND_DELTA_TIME = Reflect.Property(() => PDA.deltaTime).GetMethod;
    private static readonly MethodInfo INJECTION_OPERAND_UNSCALED_TIME = Reflect.Property(() => Time.unscaledTime).GetMethod;
    private static readonly MethodInfo INJECTION_OPERAND_UNSCALED_DELTA_TIME = Reflect.Property(() => Time.unscaledDeltaTime).GetMethod;


    /*
     * Replace all calls to PDA.time with Time.unscaledTime and to
     * PDA.deltaTime with Time.unscaledDeltaTime
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
               .MatchStartForward(new CodeMatch(OpCodes.Call, TARGET_OPERAND_TIME))
               .Repeat(matcher => matcher.SetOperandAndAdvance(INJECTION_OPERAND_UNSCALED_TIME))
               .Start()
               .MatchStartForward(new CodeMatch(OpCodes.Call, TARGET_OPERAND_DELTA_TIME))
               .Repeat(matcher => matcher.SetOperandAndAdvance(INJECTION_OPERAND_UNSCALED_DELTA_TIME))
               .InstructionEnumeration();
    }

    public override void Patch(Harmony harmony)
    {
        MethodInfo transpilerInfo = Reflect.Method(() => Transpiler(default));

        PatchTranspiler(harmony, TARGET_METHOD_ON_LATE_UPDATE, transpilerInfo);
        PatchTranspiler(harmony, TARGET_METHOD_ADD_MESSAGE, transpilerInfo);
    }
}

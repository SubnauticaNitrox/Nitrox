using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces local use of <see cref="Time.deltaTime"/> by <see cref="TimeManager.DeltaTime"/>
/// </summary>
public sealed partial class LeakingRadiation_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LeakingRadiation t) => t.Update());
    private static readonly MethodInfo INSERTED_METHOD = Reflect.Method(() => GetDeltaTime());
    private static readonly MethodInfo MATCHING_FIELD = Reflect.Property(() => Time.deltaTime).GetGetMethod();

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Call, MATCHING_FIELD))
                                            .SetOperandAndAdvance(INSERTED_METHOD)
                                            .InstructionEnumeration();
    }

    /// <summary>
    /// Wrapper for dependency resolving and variable querying
    /// </summary>
    public static float GetDeltaTime()
    {
        return Resolve<TimeManager>().DeltaTime;
    }
}

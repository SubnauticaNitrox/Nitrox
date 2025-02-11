using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using NitroxPatcher.PatternMatching.Ops;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Prevent uSkyManager from "freezing" the clouds when FreezeTime is active (game paused).
///     Also sets skybox's rotation depending on the real server time.
/// </summary>
public sealed partial class uSkyManager_SetVaryingMaterialProperties_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((uSkyManager t) => t.SetVaryingMaterialProperties(default));

    /// <summary>
    ///     Intermediate time property to simplify the dependency resolving for the transpiler.
    /// </summary>
    private static double CurrentTime => Resolve<TimeManager>().CurrentTime;

    /// <summary>
    ///     Replaces Time.time call to Time.realtimeSinceStartup so that it doesn't take Time.timeScale into account
    /// </summary>
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) => instructions
        .RewriteOnPattern(
            [
                Ldarg_0,
                Ldfld,
                PatternOp.Change(Reflect.Property(() => Time.time).GetMethod, i => i.operand = Reflect.Property(() => CurrentTime).GetMethod),
                Mul
            ]
        );
}

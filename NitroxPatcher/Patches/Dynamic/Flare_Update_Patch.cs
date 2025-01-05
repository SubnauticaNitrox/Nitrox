using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces local use of <see cref="Time.deltaTime"/> by <see cref="TimeManager.DeltaTime"/>
/// </summary>
public sealed partial class Flare_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.Update());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).ReplaceDeltaTime()
                                            .InstructionEnumeration();
    }
}

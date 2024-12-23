using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces local uses of <see cref="Time.deltaTime"/> by <see cref="TimeManager.DeltaTime"/>.
/// </summary>
public sealed partial class SeaTreader_UpdateTurning_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaTreader t) => t.UpdateTurning(out Reflect.Ref<bool>.Field));
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).ReplaceDeltaTime()
                                            .ReplaceDeltaTime()
                                            .ReplaceDeltaTime()
                                            .InstructionEnumeration();
    }
}

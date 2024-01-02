using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Keeps DevConsole disabled when enter is pressed.
/// </summary>
public sealed partial class DevConsole_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly InstructionsPattern devConsoleSetStateTruePattern = new()
    {
        Reflect.Method(() => Input.GetKeyDown(default(KeyCode))),
        Brfalse,
        Ldarg_0,
        Ldfld,
        Brtrue,
        Ldarg_0,
        { Ldc_I4_1, "ConsoleEnableFlag" },
        Reflect.Method((DevConsole t) => t.SetState(default(bool)))
    };

    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DevConsole t) => t.Update());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        return instructions.ChangeAtMarker(devConsoleSetStateTruePattern, "ConsoleEnableFlag", i => i.opcode = Ldc_I4_0);
    }
}

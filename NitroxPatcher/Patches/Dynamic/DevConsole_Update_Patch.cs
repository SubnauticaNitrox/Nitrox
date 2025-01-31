using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using NitroxPatcher.PatternMatching.Ops;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Keeps DevConsole disabled when enter is pressed.
/// </summary>
public sealed partial class DevConsole_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DevConsole t) => t.Update());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions
            .RewriteOnPattern(
                [
                    Reflect.Method(() => Input.GetKeyDown(default(KeyCode))),
                    Brfalse,
                    Ldarg_0,
                    Ldfld,
                    Brtrue,
                    Ldarg_0,
                    PatternOp.Change(Ldc_I4_1, i => i.opcode = Ldc_I4_0),
                    Reflect.Method((DevConsole t) => t.SetState(default(bool)))
                ]
            );
}

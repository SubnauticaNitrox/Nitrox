using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Keeps DevConsole disabled when enter is pressed while the Nitrox chat input is selected.
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

    private static readonly MethodInfo? shouldEnableConsoleMethod = typeof(DevConsole_Update_Patch).GetMethod(nameof(ShouldEnableConsole), BindingFlags.NonPublic | BindingFlags.Static);

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        return instructions.ChangeAtMarker(devConsoleSetStateTruePattern, "ConsoleEnableFlag", i =>
        {
            i.opcode = Call;
            i.operand = shouldEnableConsoleMethod;
        });
    }
    
    private static bool ShouldEnableConsole()
    {
        try
        {
            Type pcmType = typeof(NitroxClient.GameLogic.ChatUI.PlayerChatManager);
            object instance = pcmType.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
            PropertyInfo? isChatSelectedProp = pcmType.GetProperty("IsChatSelected", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            object val = isChatSelectedProp?.GetValue(instance);
            if (val is false)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }
}

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitGameAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.QuitGameAsync(default));

    internal static readonly object triggerOperand = Reflect.Property(() => SaveLoadManager.main).GetMethod;
    internal static readonly object injectionOperand = Reflect.Method(() => Application.Quit());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.operand != null && instruction.operand.Equals(triggerOperand))
            {
                instruction.operand = injectionOperand;
                yield return instruction;
                break;
            }

            yield return instruction;
        }
    }
}

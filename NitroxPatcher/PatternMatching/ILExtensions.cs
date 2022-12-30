using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching;

internal static class ILExtensions
{
    public static string ToPrettyString(this IEnumerable<CodeInstruction> instructions)
    {
        return string.Join(Environment.NewLine, instructions.Select(i => i.ToString()));
    }

    public static IEnumerable<CodeInstruction> Transform(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, Func<string, CodeInstruction, IEnumerable<CodeInstruction>> transform)
    {
        return pattern.ApplyTransform(instructions, transform);
    }
    
    public static IEnumerable<CodeInstruction> Transform(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, Action<string, CodeInstruction> transform)
    {
        return pattern.ApplyTransform(instructions, (label, instruction) =>
        {
            transform(label, instruction);
            return null;
        });
    }
}

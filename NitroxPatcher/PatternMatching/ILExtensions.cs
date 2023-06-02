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

    /// <summary>
    ///     Iterates the instructions, searching for the given pattern. When the pattern matches, the transform function is
    ///     called. If the pattern does not match the expected match count <see cref="InstructionsPattern.expectedMatches" />,
    ///     an exception is thrown.
    /// </summary>
    public static IEnumerable<CodeInstruction> Transform(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, Func<string, CodeInstruction, IEnumerable<CodeInstruction>> transform)
    {
        return pattern.ApplyTransform(instructions, transform);
    }

    /// <inheritdoc
    ///     cref="Transform(System.Collections.Generic.IEnumerable{HarmonyLib.CodeInstruction},NitroxPatcher.PatternMatching.InstructionsPattern,System.Func{string,HarmonyLib.CodeInstruction,System.Collections.Generic.IEnumerable{HarmonyLib.CodeInstruction}})" />
    public static IEnumerable<CodeInstruction> Transform(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, Action<string, CodeInstruction> transform)
    {
        return pattern.ApplyTransform(instructions, (label, instruction) =>
        {
            transform(label, instruction);
            return null;
        });
    }

    /// <summary>
    ///     Inserts the new instructions on every occurence of the marker, as defined by the pattern.
    /// </summary>
    /// <returns>Code with the additions.</returns>
    public static IEnumerable<CodeInstruction> InsertAfterMarker(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, string marker, CodeInstruction[] newInstructions)
    {
        return pattern.ApplyTransform(instructions, (m, _) =>
        {
            if (m.Equals(marker, StringComparison.Ordinal))
            {
                return newInstructions;
            }
            return null;
        });
    }

    /// <summary>
    ///     Calls the <paramref name="instructionChange" /> action on each instruction matching the given marker, as defined by the
    ///     pattern.
    /// </summary>
    public static IEnumerable<CodeInstruction> ChangeAtMarker(this IEnumerable<CodeInstruction> instructions, InstructionsPattern pattern, string marker, Action<CodeInstruction> instructionChange)
    {
        return pattern.ApplyTransform(instructions, (m, instruction) =>
        {
            if (m.Equals(marker, StringComparison.Ordinal))
            {
                instructionChange(instruction);
            }
            return null;
        });
    }
}

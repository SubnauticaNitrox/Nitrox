using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching;

internal static class ILExtensions
{
    private static readonly Regex spaceRegex = new(Regex.Escape(" "));

    /// <summary>
    /// Makes a string of an indexed list of instructions, line by line, formatted to have all opcodes and operand aligned in columns.
    /// </summary>
    public static string ToPrettyString(this IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = [.. instructions];
        if (instructionList.Count == 0)
        {
            return "No instructions";
        }
        int tenPower = 0;
        int count = instructionList.Count;

        while (count > 10)
        {
            count /= 10;
            tenPower++;
        }

        // if tenPower is 1 (number between 10 and 99), there are 2 numbers to show so we always add 1 to tenPower
        string format = $"D{tenPower + 1}";

        // We need to find the max length of the opcodes to have all of them take the same amount of space
        int opcodeMaxLength = 0;
        foreach (CodeInstruction instruction in instructionList)
        {
            int length = instruction.opcode.ToString().Length;
            if (length > opcodeMaxLength)
            {
                opcodeMaxLength = length;
            }
        }

        StringBuilder builder = new();
        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];
            // We add 2 so the text is more readable
            int spacesRequired = 2 + Math.Max(0, opcodeMaxLength - instruction.opcode.ToString().Length);
            string instructionToString = spaceRegex.Replace(instruction.ToString(), new string(' ', spacesRequired), 1);
            builder.AppendLine($"{i.ToString(format)}  {instructionToString}");
        }

        return builder.ToString();
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

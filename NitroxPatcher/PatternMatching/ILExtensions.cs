using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using NitroxPatcher.PatternMatching.Ops;

namespace NitroxPatcher.PatternMatching;

internal static class ILExtensions
{
    private static readonly Regex spaceRegex = new(Regex.Escape(" "));

    /// <summary>
    ///     Makes a string of an indexed list of instructions, line by line, formatted to have all opcodes and operand aligned
    ///     in columns.
    /// </summary>
    public static string ToPrettyString(this IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = [.. instructions];
        if (instructionList.Count == 0)
        {
            return "No instructions";
        }

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

        int tenPower = 0;
        int count = instructionList.Count;
        while (count > 10)
        {
            count /= 10;
            tenPower++;
        }
        string format = $"D{tenPower + 1}"; // if tenPower is 1 (number between 10 and 99), there are 2 numbers to show so we always add 1 to tenPower

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
    ///     Rewrites the instructions based on the passed in pattern and its operations.
    ///     Sub-arrays will insert new instructions at the specific position in the pattern.
    ///     Use static functions on <see cref="PatternOp"/> for more operations.
    /// </summary>
    /// <returns>The modified instructions.</returns>
    public static IEnumerable<CodeInstruction> RewriteOnPattern(this IEnumerable<CodeInstruction> instructions, PatternOp[] pattern, int expectedMatches = 1)
    {
        if (pattern is null or [])
        {
            return instructions;
        }
        IList<CodeInstruction> il = instructions as IList<CodeInstruction> ?? [..instructions];
        int matches = 0;
        // Starts from the bottom so that operations can be applied within the same loop.
        for (int ilIndex = il.Count - 1; ilIndex >= 0; ilIndex--)
        {
            // See if pattern matches entirely from current IL position.
            if (!IsPatternMatch(il, ilIndex, pattern))
            {
                continue;
            }
            matches++;
            // Match found: run operations defined in the pattern at current position.
            ApplyOperations(il, ilIndex, pattern);
        }
        if (matches != expectedMatches)
        {
            throw new Exception($"Expected {expectedMatches} matches but actual was {matches}");
        }
        return il;

        static bool IsPatternMatch(IList<CodeInstruction> il, int ilStartIndex, PatternOp[] pattern)
        {
            int ilOffset = 0;
            foreach (PatternOp op in pattern)
            {
                // Abort if trying to go out of IL range.
                if (ilStartIndex + ilOffset >= il.Count)
                {
                    return false;
                }

                CodeInstruction currentIl = il[ilStartIndex + ilOffset];
                // Type check which operation we're dealing with.
                switch (op.Op)
                {
                    case PatternOpInstruction patternIl:
                        ilOffset++;
                        if (patternIl != currentIl)
                        {
                            return false;
                        }
                        break;
                    case PatternOpCodeOpChange patternOpCodeChange:
                        ilOffset++;
                        if (patternOpCodeChange.ExpectedOpCode != currentIl.opcode)
                        {
                            return false;
                        }
                        break;
                    case PatternMethodOpChange patternMethodChange:
                        ilOffset++;
                        if (!currentIl.OperandIs(patternMethodChange.ExpectedMethod))
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        static void ApplyOperations(IList<CodeInstruction> il, int ilStartIndex, PatternOp[] pattern)
        {
            for (int ilIndex = ilStartIndex + pattern.Length - 1; ilIndex >= 0; ilIndex--)
            {
                int patternIndex = ilIndex - ilStartIndex;
                if (patternIndex <= -1)
                {
                    break;
                }

                PatternOp op = pattern[patternIndex];
                switch (op.Op)
                {
                    case PatternOpInsert insert:
                        for (int i = insert.Instructions.Count - 1; i >= 0; i--)
                        {
                            il.Insert(ilIndex, insert.Instructions[i]);
                        }
                        break;
                    case PatternOpCodeOpChange patternOpCodeChange:
                        patternOpCodeChange.Changer(il[ilIndex]);
                        break;
                    case PatternMethodOpChange patternMethodChange:
                        patternMethodChange.Changer(il[ilIndex]);
                        break;
                }
            }
        }
    }
}

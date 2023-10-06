using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching;

/// <remarks>
///     Pattern matching is NOT thread safe.
/// </remarks>
public class InstructionsPattern : IEnumerable<InstructionPattern>
{
    private readonly int expectedMatches;
    private readonly List<InstructionPattern> pattern = new();

    /// <summary>
    ///     Creates a new IL pattern to apply transforms to IL. By default, a pattern expects to match exactly once.
    /// </summary>
    public InstructionsPattern(int expectedMatches = 1)
    {
        if (expectedMatches < 1)
        {
            throw new ArgumentException($"Expected matches must be at least 1 but was {this.expectedMatches}", nameof(this.expectedMatches));
        }
        this.expectedMatches = expectedMatches;
    }

    public IEnumerator<InstructionPattern> GetEnumerator() => pattern.GetEnumerator();

    public void Add(InstructionPattern instruction)
    {
        pattern.Add(instruction);
    }

    public void Add(InstructionPattern instruction, string label)
    {
        pattern.Add(instruction with { Label = label });
    }

    public IEnumerable<CodeInstruction> ApplyTransform(IEnumerable<CodeInstruction> instructions, Func<string, CodeInstruction, IEnumerable<CodeInstruction>> transform)
    {
        CodeInstruction[] il = instructions as CodeInstruction[] ?? instructions.ToArray();
        Dictionary<int, IEnumerable<CodeInstruction>> insertOperations = new();
        int matchCount = 0;
#if DEBUG
        SetBestMatchAttemptIndex(-1);
#endif
        for (int i = 0; i < il.Length; i++)
        {
            // If pattern can't fit in remaining instructions, abort.
            if (i + pattern.Count > il.Length)
            {
                break;
            }
            // Test for pattern on current IL position.
            bool patternMatched = pattern.Count > 0;
            for (int j = 0; j < pattern.Count; j++)
            {
                CodeInstruction curInstr = il[i + j];
                InstructionPattern curInstrPattern = pattern[j];
                if (curInstr != curInstrPattern)
                {
                    patternMatched = false;
                    break;
                }
#if DEBUG
                RememberBestMatchAttempt(j);
#endif
            }
            if (!patternMatched)
            {
                continue;
            }
            matchCount++;

            // Pattern matched: now run through pattern again, adding operations at the labelled instructions.
            for (int j = 0; j < pattern.Count; j++)
            {
                if (!string.IsNullOrEmpty(pattern[j].Label))
                {
                    CodeInstruction instrAtLabel = il[i + j];
                    IEnumerable<CodeInstruction> insertingInstructions = transform(pattern[j].Label, instrAtLabel);
                    if (insertingInstructions != null)
                    {
                        insertOperations.Add(i + j, insertingInstructions);
                    }
                }
            }
        }
        if (matchCount != expectedMatches)
        {
            throw new Exception($"Expected pattern to match {expectedMatches} times but was {matchCount}. {Environment.NewLine}Pattern:{Environment.NewLine}{this}{Environment.NewLine}IL:{Environment.NewLine}{il.ToPrettyString()}");
        }

        // Apply operations on index of IL or return the original instruction.
        for (int i = 0; i < il.Length; i++)
        {
            yield return il[i];
            if (insertOperations.TryGetValue(i, out IEnumerable<CodeInstruction> inserts))
            {
                foreach (CodeInstruction newInstruction in inserts)
                {
                    yield return newInstruction;
                }
            }
        }
    }

#if DEBUG
    private int bestMatchAttemptIndex = -1;

    private void SetBestMatchAttemptIndex(int value)
    {
        bestMatchAttemptIndex = value;
    }

    private void RememberBestMatchAttempt(int value)
    {
        SetBestMatchAttemptIndex(bestMatchAttemptIndex < value ? value : bestMatchAttemptIndex);
    }
#endif

    public override string ToString() => string.Join(Environment.NewLine, pattern.Select((p, i) =>
    {
        string result = p.ToString();
#if DEBUG
        if (bestMatchAttemptIndex >= 0 && bestMatchAttemptIndex == i)
        {
            result += " <-- last matched pattern index before failure";
        }
#endif
        return result;
    }));

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

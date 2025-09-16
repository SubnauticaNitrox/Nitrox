using System.Collections.Generic;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching.Ops;

public readonly struct PatternOpInsert : IPatternOp
{
    public PatternOpInsert()
    {
    }

    public IList<CodeInstruction> Instructions { get; init; } = [];

    public static implicit operator PatternOpInsert(List<CodeInstruction> instructions) => new() { Instructions = instructions };
}

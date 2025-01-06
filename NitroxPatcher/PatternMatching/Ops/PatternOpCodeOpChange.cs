using System;
using System.Reflection.Emit;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching.Ops;

public readonly struct PatternOpCodeOpChange : IPatternOp
{
    public PatternOpCodeOpChange()
    {
    }

    public OpCode ExpectedOpCode { get; init; }
    public Action<CodeInstruction> Changer { get; init; } = _ => { };
}

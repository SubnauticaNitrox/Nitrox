using System;
using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching.Ops;

public readonly struct PatternMethodOpChange : IPatternOp
{
    public PatternMethodOpChange()
    {
    }

    public MethodInfo ExpectedMethod { get; init; }
    public Action<CodeInstruction> Changer { get; init; } = _ => { };
}

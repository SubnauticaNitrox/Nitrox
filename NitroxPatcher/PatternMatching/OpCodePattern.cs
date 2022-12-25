using System;
using System.Reflection.Emit;

namespace NitroxPatcher.PatternMatching;

public readonly struct OpCodePattern
{
    public bool Equals(OpCodePattern other) => Nullable.Equals(OpCode, other.OpCode);

    public override bool Equals(object obj) => obj is OpCodePattern other && Equals(other);

    public override int GetHashCode() => OpCode.GetHashCode();

    public OpCode? OpCode { get; init; }

    public static implicit operator OpCodePattern(OpCode opCode) => new() { OpCode = opCode };

    public static bool operator ==(OpCodePattern pattern, OpCode opCode)
    {
        return pattern.OpCode == opCode;
    }
    
    public static bool operator ==(OpCode opCode, OpCodePattern pattern)
    {
        return pattern.OpCode == opCode;
    }

    public static bool operator !=(OpCode opCode, OpCodePattern pattern)
    {
        return !(opCode == pattern);
    }

    public static bool operator !=(OpCodePattern pattern, OpCode opCode)
    {
        return !(pattern == opCode);
    }
}


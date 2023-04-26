using System;
using System.Reflection.Emit;

namespace NitroxPatcher.PatternMatching;

public readonly struct OpCodePattern
{
    public bool Equals(OpCodePattern other) => Nullable.Equals(OpCode, other.OpCode);

    public override bool Equals(object obj) => obj is OpCodePattern other && Equals(other);

    public override int GetHashCode() => OpCode.GetHashCode();

    public OpCode? OpCode { get; init; }

    /// <summary>
    ///     If true, similar opcodes will be matched as being the same.
    /// </summary>
    /// <remarks>
    ///     Example for similar opcodes (call): call, callvirt and calli.
    /// </remarks>
    public bool WeakMatch { get; init; }

    public bool IsAnyCall => WeakMatch && (OpCode == OpCodes.Call || OpCode == OpCodes.Callvirt || OpCode == OpCodes.Calli);

    public static implicit operator OpCodePattern(OpCode opCode) => new() { OpCode = opCode };

    public static bool operator ==(OpCodePattern pattern, OpCode opCode) => pattern.OpCode == opCode ||
                                                                            (pattern.IsAnyCall && (opCode == OpCodes.Call || opCode == OpCodes.Callvirt || opCode == OpCodes.Calli));
    public static bool operator ==(OpCode opCode, OpCodePattern pattern) => pattern == opCode;

    public static bool operator !=(OpCodePattern pattern, OpCode opCode) => !(pattern == opCode);
    public static bool operator !=(OpCode opCode, OpCodePattern pattern) => !(opCode == pattern);
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.PatternMatching.Ops;

public readonly record struct PatternOpInstruction() : IPatternOp
{
    public OpCodePattern OpCode { get; init; } = default;
    public object Operand { get; init; } = default;
    public List<Label> Labels { get; init; } = [];

    public static implicit operator PatternOpInstruction(MethodInfo method) => Call(method, true);

    public static PatternOpInstruction Call(MethodInfo method, bool matchAnyCallOpcode)
    {
        Type methodDeclaringType = method.DeclaringType;
        Validate.NotNull(methodDeclaringType);

        return new()
        {
            OpCode = new OpCodePattern
            {
                OpCode = OpCodes.Call,
                WeakMatch = matchAnyCallOpcode
            },
            Operand = method
        };
    }

    public static bool operator ==(PatternOpInstruction patternOp, CodeInstruction instruction)
    {
        if (instruction == null)
        {
            return false;
        }
        if (patternOp.OpCode != instruction.opcode)
        {
            return false;
        }
        if (patternOp.Operand is MethodInfo method && !instruction.OperandIs(method))
        {
            return false;
        }
        return true;
    }

    public static bool operator ==(CodeInstruction instruction, PatternOpInstruction patternOp)
    {
        return patternOp == instruction;
    }

    public static bool operator !=(CodeInstruction instruction, PatternOpInstruction patternOp)
    {
        return !(instruction == patternOp);
    }

    public static bool operator !=(PatternOpInstruction patternOp, CodeInstruction instruction)
    {
        return !(patternOp == instruction);
    }

    public bool Equals(PatternOpInstruction other) => OpCode.Equals(other.OpCode) && Operand.Equals(other.Operand);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = OpCode.GetHashCode();
            hashCode = (hashCode * 397) ^ Operand.GetHashCode();
            hashCode = (hashCode * 397) ^ (Labels != null ? Labels.GetHashCode() : 0);
            return hashCode;
        }
    }

    public override string ToString() => $"{OpCode.OpCode}{(Operand != default ? $" {Operand}" : "")}{(Labels?.Count > 0 ? $" '{Labels}'" : "")}";
}

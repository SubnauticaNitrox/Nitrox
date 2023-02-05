using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.PatternMatching;

public readonly struct InstructionPattern
{
    public bool Equals(InstructionPattern other) => OpCode.Equals(other.OpCode) && Operand.Equals(other.Operand) && Label == other.Label;

    public override bool Equals(object obj) => obj is InstructionPattern other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = OpCode.GetHashCode();
            hashCode = (hashCode * 397) ^ Operand.GetHashCode();
            hashCode = (hashCode * 397) ^ (Label != null ? Label.GetHashCode() : 0);
            return hashCode;
        }
    }

    public OpCodePattern OpCode { get; init; }
    public OperandPattern Operand { get; init; }
    public string Label { get; init; }

    public static implicit operator InstructionPattern(OpCode opCode) => new() { OpCode = opCode };
    public static implicit operator InstructionPattern(OperandPattern operand) => new() { Operand = operand };
    public static implicit operator InstructionPattern(MethodInfo method) => Call(method);

    public static InstructionPattern Call(string className, string methodName) => new() { OpCode = OpCodes.Call, Operand = new(className, methodName) };

    public static InstructionPattern Call(MethodInfo method)
    {
        Type methodDeclaringType = method.DeclaringType;
        Validate.NotNull(methodDeclaringType);

        return new() { OpCode = OpCodes.Call, Operand = new(methodDeclaringType.Name, method.Name) };
    }

    public static bool operator ==(InstructionPattern pattern, CodeInstruction instruction)
    {
        if (instruction == null)
        {
            return false;
        }
        return pattern.OpCode == instruction.opcode && pattern.Operand == instruction.operand;
    }

    public static bool operator ==(CodeInstruction instruction, InstructionPattern pattern)
    {
        return pattern == instruction;
    }

    public static bool operator !=(CodeInstruction instruction, InstructionPattern pattern)
    {
        return !(instruction == pattern);
    }

    public static bool operator !=(InstructionPattern pattern, CodeInstruction instruction)
    {
        return !(pattern == instruction);
    }

    public override string ToString() => $"{OpCode.OpCode}{(Operand != default ? $" {Operand}" : "")}{(Label != null ? $" '{Label}'" : "")}";
}

extern alias JB;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NitroxPatcher.PatternMatching.Ops;

/// <summary>
///     Pattern operation wrapper which contains the actual operation. Used to implicitly convert an input to a specific
///     operation.
/// </summary>
public struct PatternOp(IPatternOp op) : IPatternOp, IEnumerable
{
    public IPatternOp Op { get; private set; } = op;

    public static implicit operator PatternOp(OpCode opCode)
    {
        return new PatternOp { Op = new PatternOpInstruction { OpCode = opCode } };
    }

    public static implicit operator PatternOp(OperandPattern operand)
    {
        return new PatternOp { Op = new PatternOpInstruction { Operand = operand } };
    }

    public static implicit operator PatternOp(MethodInfo method)
    {
        return new PatternOp { Op = PatternOpInstruction.Call(method, true) };
    }

    public static implicit operator PatternOp(CodeInstruction instruction)
    {
        return new PatternOp
        {
            Op = new PatternOpInstruction
            {
                OpCode = instruction.opcode,
                Operand = instruction.operand,
                Labels = instruction.labels
            }
        };
    }

    public IEnumerator GetEnumerator()
    {
        throw new NotSupportedException();
    }

    public void Add(PatternOp op)
    {
        Op ??= new PatternOpInsert();
        if (Op is not PatternOpInsert insert)
        {
            throw new InvalidOperationException("Pattern operation must be an insert operation for Add() to work");
        }
        if (op.Op is not PatternOpInstruction instruction)
        {
            throw new NotSupportedException($"Only {nameof(PatternOpInstruction)} can be added to an insert operation");
        }
        if (instruction.OpCode.OpCode is not { } opCode)
        {
            throw new Exception($"{nameof(OpCode)} must not be null for new (to be inserted) instructions");
        }
        insert.Instructions.Add(new CodeInstruction(opCode, instruction.Operand));
    }

    /// <summary>
    ///     Changes the <see cref="CodeInstruction" /> if the current pattern matches entirely.
    /// </summary>
    /// <param name="expectedOpCode">The <see cref="OpCode" /> to look for at the current pattern position.</param>
    /// <param name="action">The action to invoke on the current IL position, if the pattern matched.</param>
    public static PatternOp Change(OpCode expectedOpCode, Action<CodeInstruction> action)
    {
        return new PatternOp
        {
            Op = new PatternOpCodeOpChange
            {
                ExpectedOpCode = expectedOpCode,
                Changer = action
            }
        };
    }

    /// <summary>
    ///     Changes the <see cref="CodeInstruction" /> if the current pattern matches entirely.
    /// </summary>
    /// <param name="expectedMethod">The <see cref="MethodInfo" /> to look for at the current pattern position.</param>
    /// <param name="action">The action to invoke on the current IL position, if the pattern matched.</param>
    public static PatternOp Change(MethodInfo expectedMethod, Action<CodeInstruction> action)
    {
        return new PatternOp
        {
            Op = new PatternMethodOpChange
            {
                ExpectedMethod = expectedMethod,
                Changer = action
            }
        };
    }

    public override string ToString() => Op?.ToString() ?? $"{nameof(Op)}: {Op}";
}

using System.Reflection;
using JetBrains.Annotations;

namespace NitroxPatcher.PatternMatching;

public readonly record struct OperandPattern(string DeclaringClassName, string MemberName)
{
    public bool IsAny => this == default;

    public static bool operator ==(OperandPattern pattern, [CanBeNull] object operand)
    {
        if (pattern.IsAny)
        {
            return true;
        }
        if (operand is MemberInfo member)
        {
            return pattern.DeclaringClassName == member.DeclaringType?.Name && pattern.MemberName == member.Name;
        }
        return false;
    }

    public static bool operator !=(OperandPattern pattern, object operand)
    {
        return !(pattern == operand);
    }

    public override string ToString() => $"{DeclaringClassName}{(MemberName == null ? "" : $".{MemberName}")}";
}

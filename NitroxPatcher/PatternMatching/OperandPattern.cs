using System;
using System.Reflection;
using JetBrains.Annotations;

namespace NitroxPatcher.PatternMatching;

public readonly record struct OperandPattern(string DeclaringClassName, string MemberName, Type[] ArgumentTypes = null)
{
    public bool IsAny => this == default;
    public bool IsAnyArguments => ArgumentTypes == null;

    public static bool operator ==(OperandPattern pattern, [CanBeNull] object operand)
    {
        if (pattern.IsAny)
        {
            return true;
        }
        if (operand is MemberInfo member)
        {
            if (!pattern.DeclaringClassName.Equals(member.DeclaringType?.FullName, StringComparison.OrdinalIgnoreCase) || !pattern.MemberName.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (member is MethodInfo method)
            {
                if (pattern.IsAnyArguments)
                {
                    return true;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == pattern.ArgumentTypes.Length)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (!parameters[i].ParameterType.IsAssignableFrom(pattern.ArgumentTypes[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public static bool operator !=(OperandPattern pattern, object operand)
    {
        return !(pattern == operand);
    }

    public override string ToString() => $"{DeclaringClassName}{(MemberName == null ? "" : $".{MemberName}")}";
}

extern alias JB;
using System;
using System.Runtime.CompilerServices;
using JB::JetBrains.Annotations;
using NitroxModel.DataStructures.Util;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel.Helper;

public static class Validate
{
    // "where T : class" prevents non-nullable valuetypes from getting boxed to objects.
    // In other words: Error when trying to assert non-null on something that can't be null in the first place.
    [ContractAnnotation("o:null => halt")]
    public static void NotNull<T>(T o, [CallerArgumentExpression("o")] string argumentExpression = null) where T : class
    {
        if (o != null)
        {
            return;
        }
        DisplayStatusCode(StatusCode.invalidVariableVal, false);
        throw new ArgumentNullException(argumentExpression);
    }

    public static void IsTrue(bool b, [CallerArgumentExpression("b")] string argumentExpression = null)
    {
        if (!b)
        {
            DisplayStatusCode(StatusCode.invalidVariableVal, false);
            throw new ArgumentException(argumentExpression);
        }
    }

    public static void IsFalse(bool b, [CallerArgumentExpression("b")] string argumentExpression = null)
    {
        if (b)
        {
            DisplayStatusCode(StatusCode.invalidVariableVal, false);
            throw new ArgumentException(argumentExpression);
        }
    }

    public static T IsPresent<T>(Optional<T> opt) where T : class
    {
        if (!opt.HasValue)
        {
            DisplayStatusCode(StatusCode.invalidVariableVal, false);
            throw new OptionalEmptyException<T>();
        }
        return opt.Value;
    }

    public static T IsPresent<T>(Optional<T> opt, string message) where T : class
    {
        if (!opt.HasValue)
        {
            DisplayStatusCode(StatusCode.invalidVariableVal, false);
            throw new OptionalEmptyException<T>(message);
        }
        return opt.Value;
    }
}

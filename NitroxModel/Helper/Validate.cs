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
        DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, "Value was found to be null: " + argumentExpression);
    }

    public static void IsTrue(bool b, [CallerArgumentExpression("b")] string argumentExpression = null)
    {
        if (!b)
        {
            DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, "Incorrect value was found" + argumentExpression);
        }
    }

    public static void IsFalse(bool b, [CallerArgumentExpression("b")] string argumentExpression = null)
    {
        if (b)
        {
            DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, "Incorrect value was found" + argumentExpression);
            throw new ArgumentException(argumentExpression);
        }
    }

    public static T IsPresent<T>(Optional<T> opt) where T : class
    {
        if (!opt.HasValue)
        {
            DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, "A variable was missing");
        }
        return opt.Value;
    }

    public static T IsPresent<T>(Optional<T> opt, string message) where T : class
    {
        if (!opt.HasValue)
        {
            DisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, "A variable was missing, " + message);
        }
        return opt.Value;
    }
}

using System;
using System.Runtime.CompilerServices;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Helper;

public static class Validate
{
    // "where T : class" prevents non-nullable valuetypes from getting boxed to objects.
    // In other words: Error when trying to assert non-null on something that can't be null in the first place.
    public static void NotNull<T>(T o, [CallerArgumentExpression("o")] string argumentExpression = null) where T : class
    {
        if (o != null)
        {
            return;
        }

        throw new ArgumentNullException(argumentExpression);
    }

    public static void IsTrue(bool b)
    {
        if (!b)
        {
            throw new ArgumentException();
        }
    }

    public static void IsTrue(bool b, string message)
    {
        if (!b)
        {
            throw new ArgumentException(message);
        }
    }

    public static void IsFalse(bool b)
    {
        if (b)
        {
            throw new ArgumentException();
        }
    }

    public static void IsFalse(bool b, string message)
    {
        if (b)
        {
            throw new ArgumentException(message);
        }
    }

    public static T IsPresent<T>(Optional<T> opt) where T : class
    {
        if (!opt.HasValue)
        {
            throw new OptionalEmptyException<T>();
        }
        return opt.Value;
    }

    public static T IsPresent<T>(Optional<T> opt, string message) where T : class
    {
        if (!opt.HasValue)
        {
            throw new OptionalEmptyException<T>(message);
        }
        return opt.Value;
    }
}

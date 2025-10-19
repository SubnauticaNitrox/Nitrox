using System;
using System.Collections.Generic;
using System.Linq;

namespace Nitrox.Model.Extensions;

public static class EnumExtensions
{
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);

        return type.GetField(name)
                   .GetCustomAttributes(false)
                   .OfType<TAttribute>()
                   .SingleOrDefault();
    }

    /// <inheritdoc cref="Enum.IsDefined" />
    public static bool IsDefined<TEnum>(this TEnum value) where TEnum : Enum => Enum.IsDefined(typeof(TEnum), value);

    /// <summary>
    ///     Gets only the unique flags of the given enum value that aren't part of a different flag in the same enum type,
    ///     excluding the 0 flag.
    /// </summary>
    public static IEnumerable<T> GetUniqueNonCombinatoryFlags<T>(this T flags) where T : Enum
    {
        ulong flagCursor = 1;
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if (!flags.HasFlag(value))
            {
                continue;
            }

            ulong definedFlagBits = Convert.ToUInt64(value);
            while (flagCursor < definedFlagBits)
            {
                flagCursor <<= 1;
            }

            if (flagCursor == definedFlagBits && value.HasFlag(value))
            {
                yield return value;
            }
        }
    }
}

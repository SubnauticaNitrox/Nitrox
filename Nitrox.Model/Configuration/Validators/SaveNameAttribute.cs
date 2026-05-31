using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nitrox.Model.Configuration.Validators;

/// <summary>Validates a Nitrox save name.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class SaveNameAttribute : ValidationAttribute
{
    private static readonly char[] invalidPathCharacters = Path.GetInvalidFileNameChars();

    public override bool IsValid(object value)
    {
        if (value is not string str)
        {
            return false;
        }
        int indexOfAny = str.IndexOfAny(invalidPathCharacters);
        if (indexOfAny > -1)
        {
            return false;
        }
        if (str.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}

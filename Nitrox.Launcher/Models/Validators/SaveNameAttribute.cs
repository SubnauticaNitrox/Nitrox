using System;
using System.ComponentModel.DataAnnotations;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>Validates a Nitrox save name.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class SaveNameAttribute : DataTypeAttribute
{
    public SaveNameAttribute() : base(DataType.Text)
    {
    }

    public override bool IsValid(object value)
    {
        if (value is not string str)
        {
            return false;
        }
        int indexOfAny = str.IndexOfAny(FileNameAttribute.InvalidPathCharacters);
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

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nitrox.Model.Configuration.Validators;

/// <summary>
///     Validates the value is a rooted path.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class RootedPathAttribute : ValidationAttribute
{
    public RootedPathAttribute()
    {
        ErrorMessage = "not a rooted path.";
    }

    public override bool IsValid(object value)
    {
        if (value is not string str)
        {
            return false;
        }
        if (!Path.IsPathRooted(str))
        {
            return false;
        }
        return true;
    }
}

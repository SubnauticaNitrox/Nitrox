using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nitrox.Model.Configuration.Validators;

/// <summary>
///     Validates the value is a valid and accessible directory.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DirectoryPathAttribute : ValidationAttribute
{
    public DirectoryPathAttribute()
    {
        ErrorMessage = "not a valid or accessible directory.";
    }

    public override bool IsValid(object value)
    {
        if (value is not string str)
        {
            return false;
        }
        return Directory.Exists(str);
    }
}

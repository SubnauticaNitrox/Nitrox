using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Checks that value is a usable <see cref="BackupItem" />.
/// </summary>
public sealed class BackupAttribute : TypedValidationAttribute<BackupItem>
{
    protected override ValidationResult IsValid(BackupItem value, ValidationContext context)
    {
        if (value == null)
        {
            return new ValidationResult($"{context.DisplayName} must not be null.");
        }
        if (value.BackupFileName == null || value.BackupFileName.AsSpan().Trim().IsEmpty)
        {
            return new ValidationResult($"{context.DisplayName} must have a backup path assigned");
        }
        if (!File.Exists(value.BackupFileName))
        {
            return new ValidationResult($"{context.DisplayName} must point to a valid file.");
        }
        return ValidationResult.Success;
    }
}

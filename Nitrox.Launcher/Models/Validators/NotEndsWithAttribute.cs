using System;
using System.ComponentModel.DataAnnotations;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the value doesn't end with the specified text.
/// </summary>
public sealed class NotEndsWithAttribute(string text, StringComparison comparison = StringComparison.OrdinalIgnoreCase) : TypedValidationAttribute<string>
{
    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }
        return value.EndsWith(text, comparison) ? new ValidationResult($"{context.DisplayName} must not contain the text '{text}' at the end.") : ValidationResult.Success;
    }
}

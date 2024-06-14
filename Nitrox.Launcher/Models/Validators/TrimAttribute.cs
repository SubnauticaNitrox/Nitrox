using System.ComponentModel.DataAnnotations;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the value is doesn't end with a specified character.
/// </summary>
public sealed class TrimAttribute(string trimCharacter) : TypedValidationAttribute<string>
{
    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }
        return value.EndsWith(trimCharacter) ? new ValidationResult($"{context.DisplayName} must not contain a '{trimCharacter}' at the end.") : ValidationResult.Success;
    }
}

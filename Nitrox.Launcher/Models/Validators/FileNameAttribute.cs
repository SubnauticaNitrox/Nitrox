using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the value is usable as file name (excluding validity as file path or file extension).
/// </summary>
public sealed class FileNameAttribute : TypedValidationAttribute<string>
{
    private static readonly char[] invalidPathCharacters = Path.GetInvalidFileNameChars();

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (value == null || value.IndexOfAny(invalidPathCharacters) == -1)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must be valid as a file name.");
    }
}

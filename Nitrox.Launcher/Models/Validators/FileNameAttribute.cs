using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the value is usable as file name (excluding validity as file path or file extension).
/// </summary>
public sealed class FileNameAttribute : TypedValidationAttribute<string>
{
    internal static readonly char[] InvalidPathCharacters = Path.GetInvalidFileNameChars();

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }
        int indexOfAny = value.IndexOfAny(InvalidPathCharacters);
        if (indexOfAny > -1)
        {
            return new ValidationResult($"{context.DisplayName} must not contain '{value[indexOfAny]}'. All invalid characters: {string.Join(' ', InvalidPathCharacters.Where(c => c > 31))}");
        }

        return ValidationResult.Success;
    }
}

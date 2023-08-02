using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Nitrox.Launcher.Models.Validators;

public sealed partial class ContainsNoWhitespaceAttribute : TypedValidationAttribute<string>
{
    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (value == null || WhitespaceRegex().IsMatch(value))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must not contain any spaces or new lines.");
    }
}

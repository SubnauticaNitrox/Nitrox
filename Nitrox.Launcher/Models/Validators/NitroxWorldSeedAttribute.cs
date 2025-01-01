using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Nitrox.Launcher.Models.Validators;

public sealed partial class NitroxWorldSeedAttribute : TypedValidationAttribute<string>
{
    [GeneratedRegex(@"^[a-zA-Z]{10}$")]
    private static partial Regex NitroxWorldSeedRegex { get; }

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (string.IsNullOrEmpty(value) || NitroxWorldSeedRegex.IsMatch(value))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must contain 10 alphabetical characters.");
    }
}

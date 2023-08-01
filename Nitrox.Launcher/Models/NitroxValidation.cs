using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nitrox.Launcher.Models;

public static class NitroxValidation
{
    private static readonly char[] invalidPathCharacters = Path.GetInvalidFileNameChars();
    
    public static ValidationResult IsValidFileName(string s, ValidationContext context)
    {
        if (s == null || s.All(c => !invalidPathCharacters.Contains(c)))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must not contain invalid characters.");
    }
    
    // TODO: Validate that the server name isn't a duplicate of another save
    
    public static ValidationResult ContainsNoWhiteSpace(string s, ValidationContext context)
    {
        if (s == null || !Regex.IsMatch(s, @"\s"))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must not contain any spaces.");
    }
    
    public static ValidationResult IsProperSeed(string s, ValidationContext context)
    {
        if (s == null || string.IsNullOrEmpty(s) || s.Length == 10 && Regex.IsMatch(s, @"^[a-zA-Z]+$"))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must contain 10 alphabetical characters.");
    }
    
    public static ValidationResult IsValidSaveInterval(int i, ValidationContext context)
    {
        if (i is >= 10 and <= 86400)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must be between 10s and 24 hours (86400s).");
    }
    
    public static ValidationResult IsValidPlayerLimit(int i, ValidationContext context)
    {
        if (i > 0)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {context.DisplayName} must be greater than 0.");
    }
    
}

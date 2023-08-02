using System.ComponentModel.DataAnnotations;

namespace Nitrox.Launcher.Models.Validators;

public abstract class TypedValidationAttribute<T> : ValidationAttribute
{
    protected abstract ValidationResult IsValid(T value, ValidationContext context);

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is not T typedValue)
        {
            return new ValidationResult($"The field {context.DisplayName} must be of type {typeof(T).Name}.");
        }
        return IsValid(typedValue, context);
    }
}

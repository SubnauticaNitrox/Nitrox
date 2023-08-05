using System.ComponentModel.DataAnnotations;
using System.IO;
using NitroxServer.Serialization.World;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the save name doesn't conflict with other Nitrox saves.
/// </summary>
public sealed class NitroxUniqueSaveName : TypedValidationAttribute<string>
{
    public string OriginalValuePropertyName { get; }

    public NitroxUniqueSaveName(string originalValuePropertyName = null)
    {
        OriginalValuePropertyName = originalValuePropertyName;
    }

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        if (!Directory.Exists(WorldManager.SavesFolderDir))
        {
            return ValidationResult.Success;
        }
        if (!string.IsNullOrEmpty(OriginalValuePropertyName) && value == ReadProperty<string>(context, OriginalValuePropertyName))
        {
            return ValidationResult.Success;
        }
        if (Path.Exists(Path.Combine(WorldManager.SavesFolderDir, value)))
        {
            return new ValidationResult($@"Save ""{value}"" already exists.");
        }

        return ValidationResult.Success;
    }
}

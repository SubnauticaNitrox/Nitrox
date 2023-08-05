using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using NitroxServer.Serialization.World;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the save name doesn't conflict with other Nitrox saves.
/// </summary>
public sealed class NitroxUniqueSaveName : TypedValidationAttribute<string>
{
    public bool AllowCaseInsensitiveName { get; }
    public string OriginalValuePropertyName { get; }

    public NitroxUniqueSaveName(bool allowCaseInsensitiveName = false, string originalValuePropertyName = null)
    {
        AllowCaseInsensitiveName = allowCaseInsensitiveName;
        OriginalValuePropertyName = originalValuePropertyName;
    }

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        value = value.Trim();
        
        static bool SaveFolderExists(string folderName, bool matchExact)
        {
            if (!matchExact)
            {
                foreach (string dir in Directory.EnumerateDirectories(WorldManager.SavesFolderDir))
                {
                    if (Path.GetFileName(dir).Equals(folderName, StringComparison.Ordinal)) return true;
                }
                return false;
            }

            return Path.Exists(Path.Combine(WorldManager.SavesFolderDir, folderName));
        }

        if (!Directory.Exists(WorldManager.SavesFolderDir))
        {
            return ValidationResult.Success;
        }
        if (!string.IsNullOrEmpty(OriginalValuePropertyName) && value == ReadProperty<string>(context, OriginalValuePropertyName))
        {
            return ValidationResult.Success;
        }
        if (SaveFolderExists(value, !AllowCaseInsensitiveName))
        {
            return new ValidationResult($@"Save ""{value}"" already exists.");
        }

        return ValidationResult.Success;
    }
}

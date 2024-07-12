using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nitrox.Launcher.Models.Validators;

/// <summary>
///     Tests that the save name doesn't conflict with other Nitrox saves.
/// </summary>
public sealed class NitroxUniqueSaveName : TypedValidationAttribute<string>
{
    public string SavesFolderDirPropertyName { get; }
    public bool AllowCaseInsensitiveName { get; }
    public string OriginalValuePropertyName { get; }

    public NitroxUniqueSaveName(string savesFolderDirPropertyName, bool allowCaseInsensitiveName = false, string originalValuePropertyName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(savesFolderDirPropertyName);
        SavesFolderDirPropertyName = savesFolderDirPropertyName;
        AllowCaseInsensitiveName = allowCaseInsensitiveName;
        OriginalValuePropertyName = originalValuePropertyName;
    }

    protected override ValidationResult IsValid(string value, ValidationContext context)
    {
        static bool SaveFolderExists(string folderName, bool matchExact, string savesFolderDir)
        {
            if (!matchExact)
            {
                foreach (string dir in Directory.EnumerateDirectories(savesFolderDir))
                {
                    if (Path.GetFileName(dir).Equals(folderName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                return false;
            }

            return Path.Exists(Path.Combine(savesFolderDir, folderName));
        }

        if (!Directory.Exists(ReadProperty<string>(context, SavesFolderDirPropertyName)))
        {
            return ValidationResult.Success;
        }
        if (!string.IsNullOrEmpty(OriginalValuePropertyName) && value == ReadProperty<string>(context, OriginalValuePropertyName))
        {
            return ValidationResult.Success;
        }
        if (SaveFolderExists(value, !AllowCaseInsensitiveName, ReadProperty<string>(context, SavesFolderDirPropertyName)))
        {
            return new ValidationResult($@"Save ""{value}"" already exists.");
        }

        return ValidationResult.Success;
    }
}

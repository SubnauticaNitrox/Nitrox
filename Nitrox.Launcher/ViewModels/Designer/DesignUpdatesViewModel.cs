using System;
using Nitrox.Launcher.Models.Services;

namespace Nitrox.Launcher.ViewModels.Designer;

internal sealed class DesignUpdatesViewModel() : UpdatesViewModel(null!, null!, null!, null!, null!)
{
    public new Avalonia.Collections.AvaloniaList<BackupInfo> AvailableBackups { get; } =
    [
        new BackupInfo
        {
            FilePath = "/path/to/backup1.zip",
            FileName = "nitrox_backup_1.8.0.1_2024-01-15_12-30-45.zip",
            Version = "1.8.0.1",
            CreatedAt = DateTime.Now.AddDays(-1),
            SizeBytes = 52_428_800,
            IncludesSaves = true
        },
        new BackupInfo
        {
            FilePath = "/path/to/backup2.zip",
            FileName = "nitrox_backup_1.7.0.0_2024-01-10_09-15-30.zip",
            Version = "1.7.0.0",
            CreatedAt = DateTime.Now.AddDays(-5),
            SizeBytes = 48_000_000,
            IncludesSaves = false
        }
    ];
}

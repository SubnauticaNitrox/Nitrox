using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Validators;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxServer.Serialization.World;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class BackupRestoreViewModel : ModalViewModelBase
{
    [ObservableProperty]
    private AvaloniaList<BackupItem> backups = [];

    [ObservableProperty]
    private string saveFolderDirectory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreBackupCommand))]
    [NotifyDataErrorInfo]
    [Backup]
    private BackupItem selectedBackup;

    [ObservableProperty]
    private string title;

    public BackupRestoreViewModel()
    {
        this.WhenAnyValue(model => model.SaveFolderDirectory)
            .Subscribe(owner =>
            {
                Backups.Clear();
                Backups.AddRange(GetBackups(SaveFolderDirectory));
            })
            .DisposeWith(Disposables);
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackup))]
    public void RestoreBackup()
    {
        Close();
    }

    public bool CanRestoreBackup() => !HasErrors;

    private static IEnumerable<BackupItem> GetBackups(string saveDirectory)
    {
        IEnumerable<string> GetBackupFilePaths(string backupRootDir) =>
            Directory.GetFiles(backupRootDir, "*.zip")
                     .Where(file =>
                     {
                         // Verify file name format of "Backup - {DateTime:yyyyMMddHHmmss}.zip"
                         string fileName = Path.GetFileNameWithoutExtension(file);
                         if (!fileName.StartsWith("Backup - "))
                         {
                             return false;
                         }

                         string dateTimePart = fileName["Backup - ".Length..];
                         return DateTime.TryParseExact(dateTimePart, WorldPersistence.BACKUP_DATE_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                     });

        if (saveDirectory == null)
        {
            yield break;
        }
        string backupDir = Path.Combine(saveDirectory, "Backups");
        if (!Directory.Exists(backupDir))
        {
            yield break;
        }

        foreach (string backupPath in GetBackupFilePaths(backupDir))
        {
            if (!DateTime.TryParseExact(Path.GetFileNameWithoutExtension(backupPath)["Backup - ".Length..], WorldPersistence.BACKUP_DATE_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime backupDate))
            {
                backupDate = File.GetCreationTime(backupPath);
            }
            yield return new BackupItem(backupDate, backupPath);
        }
    }
}

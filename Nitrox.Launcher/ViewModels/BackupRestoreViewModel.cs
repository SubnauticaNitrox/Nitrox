using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class BackupRestoreViewModel : ModalViewModelBase
{
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private AvaloniaList<BackupItem> backups = [];
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreBackupCommand))]
    private BackupItem selectedBackup;

    [ObservableProperty]
    private string title;
    
    [ObservableProperty]
    private string saveFolderDirectory;

    public BackupRestoreViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
        this.WhenAnyValue(model => model.SaveFolderDirectory)
            .Subscribe(owner =>
            {
                Backups.Clear();
                Backups = GetBackups(SaveFolderDirectory);
            })
            .DisposeWith(Disposables);
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackup))]
    public async Task RestoreBackup()
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            await dialogService.ShowErrorAsync(ex);
        }
        DialogResult = true;
        Close();
    }

    public bool CanRestoreBackup() => !string.IsNullOrWhiteSpace(SelectedBackup.BackupPath);

    private static AvaloniaList<BackupItem> GetBackups(string saveDirectory)
    {
        if (saveDirectory == null)
        {
            return [];
        }
        
        string backupDir = Path.Combine(saveDirectory, "Backups");
        
        if (!Directory.Exists(backupDir))
        {
            return [];
        }

        AvaloniaList<BackupItem> backupFiles = [];
        foreach (string backupPath in Directory.GetFiles(backupDir, "*.zip")
                 .Where(file =>
                 {
                     // Verify file name format of "Backup - {DateTime:yyyyMMddHHmmss}.zip"
                     string fileName = Path.GetFileNameWithoutExtension(file);
                     if (!fileName.StartsWith("Backup - "))
                     {
                         return false;
                     }
                 
                     string dateTimePart = fileName["Backup - ".Length..];
                     return DateTime.TryParseExact(dateTimePart, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _);
                 }))
        {
            DateTime backupDate;
            try
            {
                backupDate = DateTime.ParseExact(Path.GetFileNameWithoutExtension(backupPath)["Backup - ".Length..], "yyyyMMddHHmmss", null);
            }
            catch
            {
                backupDate = File.GetCreationTime(backupPath);
            }
            backupFiles.Add(new BackupItem(backupDate, backupPath));
        }
        
        return backupFiles;
    }
}

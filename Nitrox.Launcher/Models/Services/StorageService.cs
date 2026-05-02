using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Nitrox.Launcher.Models.Services;

internal sealed class StorageService
{
    // TODO: Remove this hack when Avalonia allows IStorageProvider to be accessed without demanding a Window instance.
    [field: MaybeNull, AllowNull]
    private IStorageProvider StorageProvider => field ??= ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.Windows.FirstOrDefault()?.StorageProvider ?? throw new Exception($"{nameof(IStorageProvider)} not available!");

    public async Task<string> OpenFolderPickerAsync(string title, string? startingFolder = null)
    {
        IStorageFolder startingStorageFolder = null;
        if (startingFolder != null)
        {
            startingStorageFolder = await StorageProvider.TryGetFolderFromPathAsync(startingFolder);
        }
        IReadOnlyList<IStorageFolder> dialogResult = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = startingStorageFolder
        });
        return dialogResult.FirstOrDefault()?.TryGetLocalPath() ?? "";
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options) => await StorageProvider.OpenFilePickerAsync(options);
}

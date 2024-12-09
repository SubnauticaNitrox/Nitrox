using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Nitrox.Launcher.Models.Extensions;

public static class StorageProviderExtensions
{
    public static async Task<string> OpenFolderPickerAsync(this IStorageProvider storageProvider, string title, string startingFolder = null)
    {
        IStorageFolder startingStorageFolder = null;
        if (startingFolder != null)
        {
            startingStorageFolder = await storageProvider.TryGetFolderFromPathAsync(startingFolder);
        }
        IReadOnlyList<IStorageFolder> dialogResult = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = startingStorageFolder
        });
        return dialogResult.FirstOrDefault()?.TryGetLocalPath() ?? "";
    }
}

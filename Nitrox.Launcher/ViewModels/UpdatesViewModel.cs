using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace Nitrox.Launcher.ViewModels;

public partial class UpdatesViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    private bool newUpdateAvailable;

    [ObservableProperty]
    private bool usingOfficialVersion;

    [ObservableProperty]
    private string version;

    [ObservableProperty]
    private string officialVersion;

    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];

    internal override async Task ViewContentLoadAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                NitroxChangelogs.Clear();
                NitroxChangelogs.AddRange(await Downloader.GetChangeLogsAsync());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display Nitrox changelogs");
            }
        });
    }

    public async Task<bool> IsNitroxUpdateAvailableAsync()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            Version latestVersion = await Downloader.GetNitroxLatestVersionAsync();

            NewUpdateAvailable = latestVersion > currentVersion;
#if DEBUG
            UsingOfficialVersion = false;
#else
            UsingOfficialVersion = latestVersion >= currentVersion;
#endif

            if (NewUpdateAvailable)
            {
                string versionMessage = $"A new version of the mod ({latestVersion}) is available.";
                Log.Info(versionMessage);
                LauncherNotifier.Warning(versionMessage);
            }

            Version = currentVersion.ToString();
            OfficialVersion = latestVersion.ToString();
        }
        catch // If update check fails, just show "No Update Available" text unless on debug mode
        {
            NewUpdateAvailable = false;
#if DEBUG
            UsingOfficialVersion = false;
#else
            UsingOfficialVersion = true;
#endif
        }

        return NewUpdateAvailable || !UsingOfficialVersion;
    }

    [RelayCommand]
    private void DownloadUpdate()
    {
        ProcessUtils.OpenUrl("nitrox.rux.gg/download");
    }
}

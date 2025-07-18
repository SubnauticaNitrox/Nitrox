using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace Nitrox.Launcher.ViewModels;

internal partial class UpdatesViewModel(NitroxWebsiteApiService nitroxWebsiteApi) : RoutableViewModelBase
{
    private readonly NitroxWebsiteApiService nitroxWebsiteApi = nitroxWebsiteApi;

    [ObservableProperty]
    private bool newUpdateAvailable;

    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];

    [ObservableProperty]
    private string? officialVersion;

    [ObservableProperty]
    private bool usingOfficialVersion;

    [ObservableProperty]
    private string? version;

    public async Task<bool> IsNitroxUpdateAvailableAsync()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            Version latestVersion = (await nitroxWebsiteApi.GetNitroxLatestVersionAsync()!)?.Version ?? new Version(0, 0);

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

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                NitroxChangelogs.Clear();
                NitroxChangelogs.AddRange(await nitroxWebsiteApi.GetChangeLogsAsync(cancellationToken)! ?? []);
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    LauncherNotifier.Error("Failed to fetch Nitrox changelogs");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display Nitrox changelogs");
            }
        });
    }

    [RelayCommand]
    private void DownloadUpdate() => OpenUri("nitrox.rux.gg/download");
}

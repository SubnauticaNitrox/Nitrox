using System;
using System.Diagnostics;
using System.Reactive.Disposables;
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
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class UpdatesViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    private static bool newUpdateAvailable;

    [ObservableProperty]
    private static bool usingOfficialVersion;

    [ObservableProperty]
    private static string version;

    [ObservableProperty]
    private static string officialVersion;

    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];

    public UpdatesViewModel()
    {
    }

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

    public static async Task<bool> IsNitroxUpdateAvailableAsync()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            Version latestVersion = await Downloader.GetNitroxLatestVersionAsync();

            newUpdateAvailable = latestVersion > currentVersion;
#if DEBUG
            usingOfficialVersion = false;
#else
            usingOfficialVersion = latestVersion >= currentVersion;
#endif

            if (newUpdateAvailable)
            {
                string versionMessage = $"A new version of the mod ({latestVersion}) is available.";
                Log.Info(versionMessage);
                LauncherNotifier.Warning(versionMessage);
            }

            version = currentVersion.ToString();
            officialVersion = latestVersion.ToString();
        }
        catch // If update check fails, just show "No Update Available" text unless on debug mode
        {
            newUpdateAvailable = false;
#if DEBUG
            usingOfficialVersion = false;
#else
            usingOfficialVersion = true;
#endif
        }

        return newUpdateAvailable || !usingOfficialVersion;
    }

    [RelayCommand]
    private void DownloadUpdate()
    {
        Process.Start(new ProcessStartInfo("https://nitrox.rux.gg/download") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}

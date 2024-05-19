using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public UpdatesViewModel(IScreen hostScreen) : base(hostScreen)
    {
        Dispatcher.UIThread.Invoke(new Action(async () =>
        {
            try
            {
                IList<NitroxChangelog> changelogs = await Downloader.GetChangeLogs();

                foreach (NitroxChangelog changelog in changelogs)
                {
                    nitroxChangelogs.Add(changelog);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display Nitrox changelogs");
            }
        }));
    }

    public static async Task<bool> CheckForUpdates()
    {
        try
        {
            Version currentVersion = NitroxEnvironment.Version;
            Version latestVersion = await Downloader.GetNitroxLatestVersion();

            newUpdateAvailable = latestVersion > currentVersion;
            usingOfficialVersion = latestVersion >= currentVersion;
            
            if (newUpdateAvailable)
            {
                Log.Info($"A new version of the mod ({latestVersion}) is available.");

                LauncherNotifier.Warning($"A new version of the mod ({latestVersion}) is available."); //, new ToastNotifications.Core.MessageOptions()   // TODO: Implement this?
                //{
                //    NotificationClickAction = (n) =>
                //    {
                //        MainViewModel.Router.Navigate.Execute(AppViewLocator.GetSharedViewModel< UpdatesViewModel>(););
                //    },
                //});
            }
            
            version = currentVersion.ToString();
            officialVersion = latestVersion.ToString();
        }
        catch // If update check fails, just show "No Update Available" text
        {
            newUpdateAvailable = false;
            usingOfficialVersion = true;
        }
        
        return newUpdateAvailable || !usingOfficialVersion;
    }

    [RelayCommand]
    private static void DownloadUpdate()
    {
        Process.Start(new ProcessStartInfo("https://nitrox.rux.gg/download") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}

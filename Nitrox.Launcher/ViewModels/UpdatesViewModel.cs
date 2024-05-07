using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class UpdatesViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    private bool newUpdateAvailable;
    
    [ObservableProperty]
    private string version;
    
    [ObservableProperty]
    private AvaloniaList<NitroxChangelog> nitroxChangelogs = [];
    
    public UpdatesViewModel(IScreen hostScreen) : base(hostScreen)
    {
        NewUpdateAvailable = false; // LauncherLogic.Config.IsUpToDate;
        Version = "1.8.0.0";        // LauncherLogic.Version;
        
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
                Log.Error(ex, "Error while trying to display nitrox changelogs");
            }
        }));
    }

    [RelayCommand]
    private void DownloadUpdate()
    {
        Process.Start(new ProcessStartInfo("https://nitrox.rux.gg/download") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}

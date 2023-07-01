using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages;

public partial class UpdatePage : PageBase
{
    public static readonly Uri WEBSITE_LINK = new("https://nitrox.rux.gg/download");

    public string Version => LauncherLogic.Version;

    [ObservableProperty]
    private ObservableCollection<NitroxChangelog> nitroxChangelogs = new();


    public UpdatePage()
    {
        InitializeComponent();

        Dispatcher.InvokeAsync(async () =>
        {
            CancellationTokenSource cancellationTokenSource = new(8000);
            try
            {
                IList<NitroxChangelog> changelogs = await Downloader.GetChangeLogsAsync(cancellationTokenSource.Token);
                NitroxChangelogs = new(changelogs);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display nitrox changelogs");
            }
        });

        Loaded += (s, e) =>
        {
            LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
            OnLogicPropertyChanged(null, null);
        };

        Unloaded += (s, e) =>
        {
            LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
        };
    }

    private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (LauncherLogic.Config.IsUpToDate)
        {
            UpdateAvailableBox.Visibility = Visibility.Hidden;
            NoUpdateAvailableBox.Visibility = Visibility.Visible;
        }
        else
        {
            NoUpdateAvailableBox.Visibility = Visibility.Hidden;
            UpdateAvailableBox.Visibility = Visibility.Visible;
        }
    }
}

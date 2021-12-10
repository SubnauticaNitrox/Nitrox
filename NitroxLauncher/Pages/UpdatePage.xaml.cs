using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class UpdatePage : PageBase
    {
        public static readonly Uri WEBSITE_LINK = new("https://nitrox.rux.gg/download");

        private readonly ObservableCollection<NitroxChangelog> nitroxChangelogs = new();

        public string Version => LauncherLogic.Version;

        public UpdatePage()
        {
            InitializeComponent();

            Changelogs.ItemsSource = nitroxChangelogs;

            Dispatcher?.BeginInvoke(new Action(async () =>
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
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NitroxLauncher.Models;
using NitroxModel.Logger;

namespace NitroxLauncher.Pages
{
    public partial class UpdatePage : PageBase
    {
        private readonly ObservableCollection<NitroxChangelog> nitroxChangelogs = new();

        public bool IsUpToDate => LauncherLogic.Config.IsUpToDate;
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
        }

        private void UpdatePage_Loaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
            OnPropertyChanged(nameof(IsUpToDate));
        }

        private void UpdatePage_UnLoaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(IsUpToDate));
        }
    }
}

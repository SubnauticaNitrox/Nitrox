using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using NitroxLauncher.Models;
using NitroxModel;
using NitroxModel.Discovery;

namespace NitroxLauncher.Pages
{
    public partial class LaunchGamePage : PageBase
    {
        public Platform GamePlateform => LauncherLogic.Config.SubnauticaPlatform;
        public string PlatformToolTip => GamePlateform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
        public string Version => LauncherLogic.Version;

        public LaunchGamePage()
        {
            InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Page_OnLoaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged += LogicPropertyChanged;
            OnPropertyChanged(nameof(GamePlateform));
            OnPropertyChanged(nameof(PlatformToolTip));
        }

        private void Page_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged -= LogicPropertyChanged;
        }

        private void LogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(GamePlateform));
            OnPropertyChanged(nameof(PlatformToolTip));
        }

        private async void SinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LauncherLogic.Instance.StartSingleplayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting in singleplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LauncherLogic.Instance.StartMultiplayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

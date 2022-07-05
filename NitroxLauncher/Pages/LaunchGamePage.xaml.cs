using System;
using System.ComponentModel;
using System.Windows;
using NitroxLauncher.Models;
using NitroxModel;
using NitroxModel.Discovery;

namespace NitroxLauncher.Pages
{
    public partial class LaunchGamePage : PageBase
    {
        public string PlatformToolTip => GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
        public Platform GamePlatform => LauncherLogic.Config.SubnauticaPlatform;
        public string Version => $"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";

        public LaunchGamePage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged += LogicPropertyChanged;
                LogicPropertyChanged(null, null);
            };

            Unloaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged -= LogicPropertyChanged;
            };
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

        private void LogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(GamePlatform));
            OnPropertyChanged(nameof(PlatformToolTip));
        }
    }
}

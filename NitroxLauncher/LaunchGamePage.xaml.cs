using System;
using System.Windows;
using System.Windows.Controls;

namespace NitroxLauncher
{
    public partial class LaunchGamePage : Page
    {
        private readonly LauncherLogic logic;

        public LaunchGamePage(LauncherLogic logic)
        {
            InitializeComponent();
            this.logic = logic;
        }

        private async void SinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await logic.StartSingleplayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await logic.StartMultiplayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Navigation;

namespace NitroxLauncher.Pages
{
    public partial class LaunchGamePage : PageBase
    {
        public LaunchGamePage()
        {
            InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
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
            catch(BadImageFormatException ex)
            {
                //This exception is throw with "Invalid DOS Signature" which is a clue that some
                MessageBox.Show($"Try to check your subnautica files throught Steam/EpicGames in order to fix the game: {ex.Message}", "Error, Seems like your game is corrupted :(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting in multiplayer mode", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

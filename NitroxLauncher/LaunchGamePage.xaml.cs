using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NitroxModel.Logger;

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
            Log2.Instance.LogMessage(NLogType.Info, "This is a test Message");
            Log2.Instance.LogMessage(NLogType.Info, "This is also a test Message but its reallllyyyyyy reallllyyyyyyyyy longgggggggggggggggggggg longggggggggggggggggggggggggggggggg");
            Log2.Instance.LogException("Crash!", new DivideByZeroException());
            //try
            //{
            //    await logic.StartMultiplayerAsync();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
    }
}

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using NitroxLauncher.Models;
using NitroxLauncher.Properties;

namespace NitroxLauncher.Pages
{
    public partial class ServerPage : PageBase
    {
        public string StartButtonSubtitle => $"NITROX {LauncherLogic.RELEASE_PHASE} {LauncherLogic.Version}";

        public ServerPage()
        {
            InitializeComponent();

            RBIsDocked.IsChecked = !Settings.Default.IsExternalServer;
            RBIsExternal.IsChecked = Settings.Default.IsExternalServer;
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            using (Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)))
            {
                e.Handled = true;
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LauncherLogic.Server.StartServer(RBIsExternal.IsChecked == true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RBServer_Clicked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsExternalServer = RBIsExternal.IsChecked == true;
            Settings.Default.Save();
        }
    }
}

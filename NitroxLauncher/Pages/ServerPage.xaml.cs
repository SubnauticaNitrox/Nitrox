using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
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
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void StartPublicServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LauncherLogic.Instance.StartServer(RBIsExternal.IsChecked == true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void StartPrivateServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // verify zerotier is installed
                if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ZeroTier", "One", "ZeroTier One.exe")))
                {
                    throw new Exception("Error: ZeroTier not installed, head over to the launcher settings to install ZeroTier private networking");
                }
                ServerConsolePage.isPrivateServer = true;
                LauncherLogic.Instance.StartServer(RBIsExternal.IsChecked == true, "zerotier");
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

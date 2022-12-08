using System;
using System.Windows;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class ServerPage : PageBase
    {
        public string StartButtonSubtitle => $"NITROX {LauncherLogic.ReleasePhase} {LauncherLogic.Version}";
        private bool IsServerExternal => LauncherLogic.Config.IsExternalServer;

        public ServerPage()
        {
            InitializeComponent();

            RBIsDocked.IsChecked = !IsServerExternal;
            RBIsExternal.IsChecked = IsServerExternal;
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
            LauncherLogic.Config.IsExternalServer = RBIsExternal.IsChecked ?? true;
        }
    }
}

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NitroxLauncher.Properties;
using NitroxModel;

namespace NitroxLauncher
{
    public partial class ServerPage : Page
    {
        public string Version => "NITROX ALPHA " + Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString(3);

        public ServerPage()
        {
            InitializeComponent();

            RBIsDocked.IsChecked = !Settings.Default.IsExternalServer;
            RBIsExternal.IsChecked = Settings.Default.IsExternalServer;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
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

        private void RBServer_Clicked(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsExternalServer = RBIsExternal.IsChecked == true;
            Settings.Default.Save();
        }
    }
}

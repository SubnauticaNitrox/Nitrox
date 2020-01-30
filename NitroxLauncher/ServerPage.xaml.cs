using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NitroxModel;

namespace NitroxLauncher
{
    public partial class ServerPage : Page
    {
        public string Version => "NITROX ALPHA " + Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString(3);
        private bool suppressFeedback;
        private readonly LauncherLogic logic;

        public ServerPage(LauncherLogic logic)
        {
            suppressFeedback = true;
            InitializeComponent();

            RBIsDocked.IsChecked = !Properties.Settings.Default.IsExternalServer;
            RBIsExternal.IsChecked = Properties.Settings.Default.IsExternalServer;

            suppressFeedback = false;
            this.logic = logic;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logic.StartServer((bool)RBIsExternal.IsChecked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RBServer_Checked(object sender, RoutedEventArgs e)
        {
            if (!suppressFeedback)
            {
                Properties.Settings.Default.IsExternalServer = (bool)RBIsExternal.IsChecked;
                Properties.Settings.Default.Save();
            }
        }
    }
}

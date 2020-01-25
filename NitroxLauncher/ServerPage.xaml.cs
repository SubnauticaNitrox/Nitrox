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
        private bool embeddedServer;
        private readonly LauncherLogic logic;

        public ServerPage(LauncherLogic logic)
        {
            InitializeComponent();

            // Change style depending on windows version. Win 10 uses other definition of comboboxes then win 7 so win 10 has its own style
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1)
            {
                CBox.Style = (Style)Resources["ComboBoxStyle"];
                CBox.ApplyTemplate();
            }
            this.logic = logic;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logic.StartServer(!embeddedServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            ComboBoxItem item = (ComboBoxItem)box.SelectedValue;
            
            embeddedServer = item.Tag.ToString() == "embedded";
        }
    }
}

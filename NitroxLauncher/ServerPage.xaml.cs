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

            foreach (ComboBoxItem boxItem in CBox.Items)
            {
                if ( (Properties.Settings.Default.IsEmbeddedServer && boxItem.Tag.ToString() == "embedded") ||
                    (!Properties.Settings.Default.IsEmbeddedServer && boxItem.Tag.ToString() != "embedded"))
                {
                    CBox.SelectedItem = boxItem;
                }
            }

            suppressFeedback = false;

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
                logic.StartServer(!Properties.Settings.Default.IsEmbeddedServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (!suppressFeedback)
            {
                ComboBox box = (ComboBox)sender;
                ComboBoxItem item = (ComboBoxItem)box.SelectedValue;
                Properties.Settings.Default.IsEmbeddedServer = item.Tag.ToString() == "embedded";
                Properties.Settings.Default.Save();
            }
        }
    }
}

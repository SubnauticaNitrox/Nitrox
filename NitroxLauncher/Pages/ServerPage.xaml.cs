using System;
using System.Windows;
using System.Windows.Controls;
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

            // If the "Display Server Console Externally" Checkbox is checked, set value to true
            if (CBIsExternal.IsChecked == true)
            {
                CBIsExternal.IsChecked = IsServerExternal;
            }
            else
            {
                CBIsExternal.IsChecked = !IsServerExternal;
            }
            
            
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            // If the "Start Server button" is selected and not the "Display Server Console Externally" Checkbox, then start the server
            if (!(e.OriginalSource is CheckBox))
            {
                try
                {
                    LauncherLogic.Server.StartServer(CBIsExternal.IsChecked == true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CBServer_Clicked(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.IsExternalServer = CBIsExternal.IsChecked ?? true;
        }

        private void RBGamemode_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        /* Restore Backup Button (WIP)
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            //e.Handled = true; // PUT THIS LINE IN THE CODE TO PREVENT THE OUTER BUTTON FROM BEING ACTIVATED IF BUTTON IS IMBEDDED IN ANOTHER BUTTON

        }
        */
    }
}

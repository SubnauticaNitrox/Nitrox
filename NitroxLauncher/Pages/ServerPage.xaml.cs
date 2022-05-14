using System;
using System.Windows;
using System.Windows.Controls;
using NitroxLauncher.Models;
using NitroxModel.Core;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxLauncher.Pages
{
    public partial class ServerPage : PageBase
    {
        public string StartButtonSubtitle => $"NITROX {LauncherLogic.ReleasePhase} {LauncherLogic.Version}";
        public bool IsServerExternal
        {
            get => LauncherLogic.Config.IsExternalServer;
            set => LauncherLogic.Config.IsExternalServer = value;
        }
        public string PathToSubnautica => LauncherLogic.Config.SubnauticaPath;
        //private readonly ServerConfig serverConfig;                 // Taken from Server.cs (DOESN'T WORK, Troubleshoot this)

        // Variables for server config manipulation
        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        public bool enableCheatsValue => true;
        public bool enablePvPValue => true;
        public bool enableAutoPortForwardValue => true;
        public bool enableFullEntityCacheValue => false;
        public int serverPort => 11000;//serverConfig?.ServerPort ?? -1;    // Taken from Server.cs (DOESN'T WORK: Displays -1)

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

        //private void CBServer_Clicked(object sender, RoutedEventArgs e)
        //{
        //    IsServerExternal = CBIsExternal.IsChecked ?? true;
        //}

        private void AddWorld_Click(object sender, RoutedEventArgs e)
        {
            //Note: DOES NOT WORK v
            //NitroxServiceLocator.LocateService<WorldPersistence>().CreateFreshWorld();
        }

        private void RBGamemode_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void CBCheats_Enabled(object sender, RoutedEventArgs e)
        {
            //enableCheatsValue = CBCheats.IsChecked;
        }

        private void CBPvP_Enabled(object sender, RoutedEventArgs e)
        {

        }

        private void CBAutoPortForward_Enabled(object sender, RoutedEventArgs e)
        {

        }

        private void CBCreateFullEntityCache_Enabled(object sender, RoutedEventArgs e)
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

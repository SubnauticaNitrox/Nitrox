using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NitroxLauncher.Models;
using NitroxModel.Core;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxLauncher.Pages
{
    public partial class ServerPage : PageBase
    {
        public string LauncherVersion => $"{LauncherLogic.ReleasePhase} v{LauncherLogic.Version}";
        public bool IsServerExternal
        {
            get => LauncherLogic.Config.IsExternalServer;
            set => LauncherLogic.Config.IsExternalServer = value;
        }
        
        public string PathToSubnautica => LauncherLogic.Config.SubnauticaPath;
        public static string SavesFolderDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Nitrox\\saves";
        readonly System.IO.DirectoryInfo savesFolderDir = new(SavesFolderDir); //add public at front

        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        // World settings variables (TODO: REMOVE THESE)
        private readonly ServerConfig serverConfig = ServerConfig.Load();

        public string SelectedWorldName { get; set; }
        public string SelectedWorldSeed { get; set; }
        public string SelectedWorldVersion { get; set; }
        public ServerGameMode SelectedWorldGamemode { get; set; }
        public bool EnableCheatsValue { get; set; }
        public bool EnableAutoPortForwardValue { get; set; }
        public bool EnableFullEntityCacheValue { get; set; }
        public bool EnableLanDiscoveryValue { get; set; }
        public int ServerPort { get; set; }

        public bool IsNewWorld { get; set; }

        public int SelectedWorldIndex { get; set; }

        public ServerPage()
        {
            InitializeComponent();

            // Look for or create the "saves" folder
            if (!Directory.Exists(SavesFolderDir))
            {
                Log.Info($"Could not find a \"saves\" folder, creating one at {SavesFolderDir}");
                Directory.CreateDirectory(SavesFolderDir);
            }
            else
            {
                Log.Info($"Found a \"saves\" folder at {SavesFolderDir}");
            }

            // Initialize a new list
            List<World_Listing> WorldListing = new();

            // Get number of files in the "saves" folder
            int numFiles = savesFolderDir.GetDirectories().Length;
            Log.Info($"There are {numFiles} folders in the saves folder");

            // Add each save file into the list if it's a valid save file
            for (int i = 0; i < numFiles; ++i)
            {
                
            }

            // TEMPORARY - Manually added listing items for testing
            WorldListing.Add(new World_Listing() { WorldName = "Survival w/ friends", WorldGamemode = "Survival", WorldVersion = "v1.6.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "My World 1", WorldGamemode = "Freedom", WorldVersion = "v1.5.0.2" });
            WorldListing.Add(new World_Listing() { WorldName = "v1.7 pre-release test", WorldGamemode = "Freedom", WorldVersion = "v1.7.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "Old World", WorldGamemode = "Survival", WorldVersion = "v1.3.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "Creative Mode", WorldGamemode = "Creative", WorldVersion = "v1.6.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "v1.6.0.1 testing", WorldGamemode = "Freedom", WorldVersion = "v1.6.0.1" });

            // Bind the list data to be used in XAML
            WorldListingContainer.ItemsSource = WorldListing;

            // If the "Display Server Console Externally" Checkbox is checked, set value to true - (Is this needed anymore?)
            if (CBIsExternal.IsChecked == true)
            {
                CBIsExternal.IsChecked = IsServerExternal;
            }

        }
        
        // File Management
        private bool ValidateSave(int saveFileNum)
        {
            bool saveValidity = false;

            return saveValidity;
        }

        public void SaveConfigSettings()
        {
            serverConfig.Update(c =>
            {
                c.SaveName = SelectedWorldName;

                if (IsNewWorld == true) { c.Seed = SelectedWorldSeed; }

                if (RBFreedom.IsChecked == true) { c.GameMode = ServerGameMode.FREEDOM; }
                else if (RBSurvival.IsChecked == true) { c.GameMode = ServerGameMode.SURVIVAL; }
                else if (RBCreative.IsChecked == true) { c.GameMode = ServerGameMode.CREATIVE; }

                c.DisableConsole = !EnableCheatsValue;
                c.AutoPortForward = EnableAutoPortForwardValue;
                c.CreateFullEntityCache = EnableFullEntityCacheValue;
                c.LANDiscoveryEnabled = EnableLanDiscoveryValue;
                c.ServerPort = ServerPort;

            });
            Log.Info($"Server Config updated");
        }

        // Pane Buttons
        public void AddWorld_Click(object sender, RoutedEventArgs e)
        {
            IsNewWorld = true;

            ServerConfig serverConfig = ServerConfig.Load();

            // THESE WOULD BE SET TO DEFAULT VALUES INSTEAD OF SERVER.CFG VALUES, BUT THEY ARE USED HERE UNTIL I CAN FIGURE OUT THE WORLD SELECTION BUTTONS
            SelectedWorldName = serverConfig.SaveName;
            SelectedWorldSeed = serverConfig.Seed;
            SelectedWorldGamemode = serverConfig.GameMode;
            EnableCheatsValue = !serverConfig.DisableConsole;
            EnableAutoPortForwardValue = serverConfig.AutoPortForward;
            EnableFullEntityCacheValue = serverConfig.CreateFullEntityCache;
            EnableLanDiscoveryValue = serverConfig.LANDiscoveryEnabled;
            ServerPort = serverConfig.ServerPort;

            // Set the world settings values to the server.cfg values
            TBWorldName.Text = SelectedWorldName;
            TBWorldSeed.Text = SelectedWorldSeed;
            if (SelectedWorldGamemode == ServerGameMode.FREEDOM) { RBFreedom.IsChecked = true; }
            else if (SelectedWorldGamemode == ServerGameMode.SURVIVAL) { RBSurvival.IsChecked = true; }
            else if (SelectedWorldGamemode == ServerGameMode.CREATIVE) { RBCreative.IsChecked = true; }
            CBCheats.IsChecked = EnableCheatsValue;
            CBAutoPortForward.IsChecked = EnableAutoPortForwardValue;
            CBCreateFullEntityCache.IsChecked = EnableFullEntityCacheValue;
            CBLanDiscovery.IsChecked = EnableLanDiscoveryValue;
            TBWorldServerPort.Text = Convert.ToString(ServerPort);

            TBWorldSeed.IsEnabled = true;

            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigSettings();

            Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
            GoBackAnimationStoryboard.Begin();
        }

        // World management
        public void WorldListing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            SelectedWorldIndex = WorldListingContainer.SelectedIndex;
            Log.Info($"World index {SelectedWorldIndex} selected");

        }

        private void SelectWorld_Click(object sender, RoutedEventArgs e)
        {
            IsNewWorld = false;
            TBWorldSeed.IsEnabled = false;

            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

            //SelectedWorldIndex
        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {

        }

        // World settings management (MAY REMOVE THESE

        private void TBWorldName_Changed(object sender, TextChangedEventArgs e)
        {
            SelectedWorldName = TBWorldName.Text;
            Log.Info($"World name set to {SelectedWorldName}");
        }

        private void TBWorldSeed_Changed(object sender, TextChangedEventArgs e)
        {
            SelectedWorldSeed = TBWorldSeed.Text;
            Log.Info($"World seed set to {SelectedWorldSeed}");
        }

        private void RBGamemode_Clicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void CBCheats_Clicked(object sender, RoutedEventArgs e)
        {
            EnableCheatsValue = (bool)CBCheats.IsChecked;
            Log.Info($"DisableConsole set to {EnableCheatsValue}");
        }

        private void CBLanDiscovery_Clicked(object sender, RoutedEventArgs e)
        {
            EnableLanDiscoveryValue = (bool)CBLanDiscovery.IsChecked;
            Log.Info($"LanDiscovery set to {EnableLanDiscoveryValue}");
        }

        private void CBAutoPortForward_Clicked(object sender, RoutedEventArgs e)
        {
            EnableAutoPortForwardValue = (bool)CBAutoPortForward.IsChecked;
            Log.Info($"AutoPortForward set to {EnableAutoPortForwardValue}");
        }

        private void CBCreateFullEntityCache_Clicked(object sender, RoutedEventArgs e)
        {
            EnableFullEntityCacheValue = (bool)CBCreateFullEntityCache.IsChecked;
            Log.Info($"CreateFullEntityCache set to {EnableFullEntityCacheValue}");
        }

        // RUNS EVEN IF TEXT IS CHANGED DURING STARTUP - MUST CHANGE!!! (?)
        private void TBWorldServerPort_Changed(object sender, TextChangedEventArgs e)
        {
            int ServerPortNum = 11000;
            try
            {
                ServerPortNum = Convert.ToInt32(TBWorldServerPort.Text);
            }
            catch
            {
                Log.Info($"ServerPort input not valid");
            }
            
            ServerPort = ServerPortNum;
        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        // Start server button management
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigSettings();

            Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
            GoBackAnimationStoryboard.Begin();

            // If the "Start Server button" is clicked and not the "Display Server Console Externally" Checkbox, then start the server
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

        /* Restore Backup Button (WIP)
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            //e.Handled = true; // PUT THIS LINE IN THE CODE TO PREVENT THE OUTER BUTTON FROM BEING ACTIVATED IF BUTTON IS IMBEDDED IN ANOTHER BUTTON

        }
        */

    }


    public class World_Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode { get; set; }
        public string WorldVersion { get; set; }
        public string WorldSavePath { get; set; }
    }

}

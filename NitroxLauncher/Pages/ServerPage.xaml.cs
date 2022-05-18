using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NitroxLauncher.Models;
using NitroxModel.Core;
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
        readonly System.IO.DirectoryInfo savesFolderDir = new DirectoryInfo(SavesFolderDir);
        private readonly ServerConfig serverConfig = ServerConfig.Load();

        // Variables for server config manipulation
        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        public string selectedWorldName => "My World";
        public string selectedWorldSeed => "QTUJNDGRBK";
        public string selectedWorldVersion => "v1.6.0.1";

        // THESE CAN'T BE LOCATED HERE
        public bool enableCheatsValue => !serverConfig.DisableConsole;
        public bool enablePvPValue => true;
        public bool enableAutoPortForwardValue => serverConfig.AutoPortForward;
        public bool enableFullEntityCacheValue => serverConfig.CreateFullEntityCache;

        public int serverPort => 11000;//serverConfig?.ServerPort ?? -1;    // Taken from Server.cs (DOESN'T WORK: Displays -1)
        public int SelectedWorldIndex;


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

            // If the "Display Server Console Externally" Checkbox is checked, set value to true
            if (CBIsExternal.IsChecked == true)
            {
                CBIsExternal.IsChecked = IsServerExternal;
            }

        }
        
        //File Management
        private bool ValidateSave(int saveFileNum)
        {
            bool saveValidity = false;



            return saveValidity;
        }

        // Pane Buttons
        private void AddWorld_Click(object sender, RoutedEventArgs e)
        {
            //enableCheatsValue = true;
            //public bool enablePvPValue => true;
            //public bool enableAutoPortForwardValue => true;
            //public bool enableFullEntityCacheValue => false;


            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

            //Note: DOES NOT WORK v
            //NitroxServiceLocator.LocateService<WorldPersistence>().CreateFreshWorld();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            serverConfig.Update();//(c =>
            //{
            //    c.
            //});
            Log.Info($"Server Config updated");

            
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
            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

            //SelectedWorldIndex
        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {

        }

        // World settings management
        private void RBGamemode_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void CBCheats_Clicked(object sender, RoutedEventArgs e)
        {
            //enableCheatsValue = CBCheats.IsChecked;
            serverConfig.DisableConsole = (bool)!CBCheats.IsChecked;
            Log.Info($"DisableConsole set to {serverConfig.DisableConsole}");
        }

        private void CBPvP_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void CBAutoPortForward_Clicked(object sender, RoutedEventArgs e)
        {
            serverConfig.AutoPortForward = (bool)CBAutoPortForward.IsChecked;
            Log.Info($"AutoPortForward set to {serverConfig.AutoPortForward}");
        }

        private void CBCreateFullEntityCache_Clicked(object sender, RoutedEventArgs e)
        {
            serverConfig.CreateFullEntityCache = (bool)CBCreateFullEntityCache.IsChecked;
            Log.Info($"CreateFullEntityCache set to {serverConfig.CreateFullEntityCache}");
        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        // Start server button management
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
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

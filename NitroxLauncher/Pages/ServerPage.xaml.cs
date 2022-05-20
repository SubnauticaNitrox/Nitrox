using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using NitroxLauncher.Models;
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
        public static string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");
        readonly System.IO.DirectoryInfo savesFolderDir = new(SavesFolderDir); //add public at front

        // World settings variables (TODO: REMOVE THESE)
        //private readonly ServerConfig serverConfig = ServerConfig.Load();

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

        //public int SelectedWorldIndex { get; set; }
        public string SelectedWorldDirectory { get; set; }

        public List<World_Listing> WorldListing { get; set; }

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

            InitializeWorldListing();

            // If the "Display Server Console Externally" Checkbox is checked, set value to true - (Is this needed anymore?)
            if (CBIsExternal.IsChecked == true)
            {
                CBIsExternal.IsChecked = IsServerExternal;
            }

        }
        
        public void InitializeWorldListing()
        {
            // Initialize a new list
            WorldListing = new();

            // Get number of files in the "saves" folder
            //int numFiles = savesFolderDir.GetDirectories().Length;
            //Log.Info($"There are {numFiles} folders in the saves folder");


            // Add each save file into the list if it's a valid save file   -   (NEED TO FIX THIS - Save files with numbers higher than the amount of save files there are don't get read and validated)  -  IGNORE FILE NAMES
            if (savesFolderDir.GetDirectories().Length > 0)
            {
                //for (int i = 0; i < numFiles; ++i)
                foreach (string folder in Directory.GetDirectories(SavesFolderDir))
                {
                    if (ValidateSave(folder))
                    {
                        ServerConfig serverConfig = ServerConfig.Load(folder);

                        //SaveFileVersion worldVersion = new ServerJsonSerializer().Deserialize<SaveFileVersion>(Path.Combine(folder, "Version.json"));
                        WorldListing.Add(new World_Listing() 
                        { 
                            WorldName = serverConfig.SaveName,
                            WorldGamemode = Convert.ToString(serverConfig.GameMode), 
                            WorldVersion =  "v1.6.0.1", //VersionToString(worldVersion), NEEDS SOME FIXING
                            WorldSaveDir = folder
                        });
                        Log.Info($"Found a valid save file at: {folder}");
                    }
                }
            }

            /*
            // TEMPORARY - Manually added listing items for testing
            WorldListing.Add(new World_Listing() { WorldName = "Survival w/ friends", WorldGamemode = "Survival", WorldVersion = "v1.6.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "My World 1", WorldGamemode = "Freedom", WorldVersion = "v1.5.0.2" });
            WorldListing.Add(new World_Listing() { WorldName = "v1.7 pre-release test", WorldGamemode = "Freedom", WorldVersion = "v1.7.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "Old World", WorldGamemode = "Survival", WorldVersion = "v1.3.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "Creative Mode", WorldGamemode = "Creative", WorldVersion = "v1.6.0.0" });
            WorldListing.Add(new World_Listing() { WorldName = "v1.6.0.1 testing", WorldGamemode = "Freedom", WorldVersion = "v1.6.0.1" });
            */

            // Set the background of the WorldSelectionContainer to be the "No Worlds found" message and the server image if the listing has no entries
            if (WorldListing.Count == 0)
            {
                NoWorldsBackground.Opacity = 1;
            }
            else
            {
                NoWorldsBackground.Opacity = 0;
            }

            // Bind the list data to be used in XAML
            WorldListingContainer.ItemsSource = WorldListing;
        }

        // File Management
        private bool ValidateSave(string fileName)
        {
            // A save file is valid when it's named "save#", has a "server.cfg" file, and has all of the nested save file names in it
            string saveDir = Path.Combine(SavesFolderDir, fileName);
            string serverConfigFile = Path.Combine(saveDir, "server.cfg");

            string[] filesToCheck = {
                "BaseData",
                "EntityData",
                "PlayerData",
                "Version",
                "WorldData"
            };

            if (!Directory.Exists(saveDir) || !File.Exists(serverConfigFile))
            {
                return false;
            }
            foreach (string file in filesToCheck)
            {
                if (!File.Exists(Path.Combine(saveDir, Path.ChangeExtension(file, "json"))) && !File.Exists(Path.Combine(saveDir, Path.ChangeExtension(file, "NITROX"))))
                {
                    return false;
                }
            }

            return true;

        }

        public void SaveConfigSettings()
        {
            ServerConfig serverConfig = ServerConfig.Load(SelectedWorldDirectory);
            serverConfig.Update(SelectedWorldDirectory, c =>
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

        public void UpdateVisualWorldSettings()
        {
            //string saveDir = Path.Combine(SavesFolderDir, "save" + saveFileNum);
            ServerConfig serverConfig = ServerConfig.Load(SelectedWorldDirectory);

            // Get config file values
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
        }

        // Pane Buttons
        public void AddWorld_Click(object sender, RoutedEventArgs e)
        {
            Log.Info($"Adding new world");
            IsNewWorld = true;
            TBWorldSeed.IsEnabled = true;

            //SelectedWorldIndex = WorldListing.Count + 1;
            //SelectedWorldDirectory = Path.Combine(SavesFolderDir, "save" + SelectedWorldIndex);
            string newSaveFileDir = Path.Combine(SavesFolderDir, "save0");

            for (int i = 1; Directory.Exists(newSaveFileDir); i++)
            {
                newSaveFileDir = Path.Combine(SavesFolderDir, "save" + i);
            }

            Directory.CreateDirectory(newSaveFileDir);

            SelectedWorldDirectory = newSaveFileDir;
            UpdateVisualWorldSettings();

            ServerConfig serverConfig = ServerConfig.Load(SelectedWorldDirectory);
            
            string[] defaultSaveFiles = {
                "BaseData",
                "EntityData",
                "PlayerData",
                "Version",
                "WorldData"
            };
            string fileEnding = ".json";
            if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
            {
                fileEnding = ".nitrox";
            }

            foreach (string file in defaultSaveFiles)
            {
                File.Create(Path.Combine(newSaveFileDir, file + fileEnding));
            }
            
            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigSettings();
            InitializeWorldListing();

            Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
            GoBackAnimationStoryboard.Begin();
        }

        // World management
        private void SelectWorld_Click(object sender, RoutedEventArgs e)
        {
            IsNewWorld = false;
            TBWorldSeed.IsEnabled = false;

            Log.Info($"World index {WorldListingContainer.SelectedIndex} selected");

            SelectedWorldDirectory = WorldListing[WorldListingContainer.SelectedIndex].WorldSaveDir;

            UpdateVisualWorldSettings();

            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();

        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationBox.Opacity = 1;
            ConfirmationBox.IsHitTestVisible = true;
        }

        private void YesConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorldDirectory = WorldListing[WorldListingContainer.SelectedIndex].WorldSaveDir;
            Directory.Delete(SelectedWorldDirectory, true);
            Log.Info($"Deleting world index {WorldListingContainer.SelectedIndex}");

            ConfirmationBox.Opacity = 0;
            ConfirmationBox.IsHitTestVisible = false;
            InitializeWorldListing();
        }

        private void NoConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationBox.Opacity = 0;
            ConfirmationBox.IsHitTestVisible = false;
            InitializeWorldListing();
        }

        // World settings management (MAY REMOVE THESE)
        private void TBWorldName_Changed(object sender, TextChangedEventArgs e)
        {
            // CHECK THAT THE STRING ISN'T EMPTY, OR THE LAUNCHER WILL CRASH HERE
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
            // If the "Start Server button" is clicked and not the "Display Server Console Externally" Checkbox, then start the server
            if (!(e.OriginalSource is CheckBox))
            {
                SaveConfigSettings();
                InitializeWorldListing();

                try
                {
                    LauncherLogic.Server.StartServer(CBIsExternal.IsChecked == true, SelectedWorldDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
                GoBackAnimationStoryboard.Begin();
            }

        }

        /* Restore Backup Button (WIP)
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            //e.Handled = true; // PUT THIS LINE IN THE CODE TO PREVENT THE OUTER BUTTON FROM BEING ACTIVATED IF BUTTON IS IMBEDDED IN ANOTHER BUTTON

        }
        */

        public string VersionToString(SaveFileVersion version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

    }

    public class World_Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode { get; set; }
        public string WorldVersion { get; set; }
        public string WorldSaveDir { get; set; }
    }

}

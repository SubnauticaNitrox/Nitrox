using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        // World settings variables (TODO: REMOVE THESE)

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
        
        public ServerPage()
        {
            InitializeComponent();
            InitializeWorldListing();

            // If the "Display Server Console Externally" Checkbox is checked, set value to true - (Is this needed anymore?)
            if (CBIsExternal.IsChecked == true)
            {
                CBIsExternal.IsChecked = IsServerExternal;
            }
        }
        
        public void InitializeWorldListing()
        {
            WorldManager.Refresh();

            NoWorldsBackground.Opacity = WorldManager.GetSaves().Any() ? 0 : 1;

            // Bind the list data to be used in XAML
            WorldListingContainer.ItemsSource = null;
            WorldListingContainer.ItemsSource = WorldManager.GetSaves();
        }

        // File Management
        public void SaveConfigSettings()
        {
            // If world name was changed, rename save folder to match it
            string dest = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), SelectedWorldName);
            if (SelectedWorldDirectory != dest)
            {
                Directory.Move(SelectedWorldDirectory, dest+" temp"); // These two lines are need to handle names that change in
                Directory.Move(dest+" temp", dest);                   // case, since windows still thinks of the two names as the same
                SelectedWorldDirectory = dest;
            }

            ServerConfig serverConfig = ServerConfig.Load(SelectedWorldDirectory);
            serverConfig.Update(SelectedWorldDirectory, c =>
            {
                c.SaveName = SelectedWorldName;
                if (IsNewWorld) { c.Seed = SelectedWorldSeed; }
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
            SelectedWorldName = Path.GetFileName(SelectedWorldDirectory);
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

            SelectedWorldDirectory = WorldManager.CreateEmptySave("My World");
            UpdateVisualWorldSettings();

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

            SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";

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
            SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";
            Directory.Delete(SelectedWorldDirectory, true);
            Log.Info($"Deleting world \"{SelectedWorldName}\"");

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
        private void TBWorldName_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBWorldName.Text = TBWorldName.Text.TrimStart();
            TBWorldName.Text = TBWorldName.Text.TrimEnd();

            // Make sure the string isn't empty
            if (!string.IsNullOrEmpty(TBWorldName.Text))
            {
                // Make sure the world name is valid as a file name
                bool isIllegalName = false;
                char[] illegalChars = Path.GetInvalidFileNameChars();
                for (int i = 0; !isIllegalName && i < illegalChars.Length; i++)
                {
                    isIllegalName = TBWorldName.Text.Contains(illegalChars[i]);
                }

                if (isIllegalName)
                {
                    TBWorldName.Text = string.Join("_", TBWorldName.Text.Split(Path.GetInvalidFileNameChars()));
                    LauncherNotifier.Error("World names cannot contain these characters: < > : \" / \\ | ? *");
                }

                // Check that name is not a duplicate if it was changed
                string newSelectedWorldDirectory = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), TBWorldName.Text);
                if (!newSelectedWorldDirectory.Equals(SelectedWorldDirectory, StringComparison.OrdinalIgnoreCase) && Directory.Exists(newSelectedWorldDirectory))
                {
                    LauncherNotifier.Error($"World name \"{TBWorldName.Text}\" already exists.");

                    int i = 1;
                    Regex rx = new(@"\((\d+)\)$");
                    if (!rx.IsMatch(TBWorldName.Text))
                    {
                        SelectedWorldName = TBWorldName.Text + $" ({i})";
                        newSelectedWorldDirectory = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), SelectedWorldName);
                    }

                    while (Directory.Exists(newSelectedWorldDirectory) && !newSelectedWorldDirectory.Equals(SelectedWorldDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        // Increment the number to the end of the name until it reaches an available filename
                        SelectedWorldName = rx.Replace(SelectedWorldName, $"({i})", 1);
                        newSelectedWorldDirectory = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), SelectedWorldName);
                        i++;
                    }

                    TBWorldName.Text = SelectedWorldName;
                }
                SelectedWorldName = TBWorldName.Text;
                Log.Info($"World name set to \"{SelectedWorldName}\".");
            }
            else
            {
                TBWorldName.Text = SelectedWorldName;
                LauncherNotifier.Error($"An empty world name is not valid.");
            }
        }

        private void TBWorldSeed_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBWorldSeed.Text = TBWorldSeed.Text.TrimStart();
            TBWorldSeed.Text = TBWorldSeed.Text.TrimEnd();
            TBWorldSeed.Text = TBWorldSeed.Text.ToUpper();

            if (TBWorldSeed.Text.Length != 10 || !Regex.IsMatch(TBWorldSeed.Text, @"^[a-zA-Z]+$"))
            {
                TBWorldSeed.Text = SelectedWorldSeed;
                LauncherNotifier.Error($"World Seeds should contain 10 alphabetical characters (A-Z).");
                return;
            }
            
            SelectedWorldSeed = TBWorldSeed.Text;
            Log.Info($"World seed set to {SelectedWorldSeed}");
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

        private void TBWorldServerPort_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimStart();
            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBWorldServerPort.Text))
            {
                TBWorldServerPort.Text = Convert.ToString(ServerPort);
                LauncherNotifier.Error($"An empty ServerPort input is not valid.");
                return;
            }

            int ServerPortNum;
            try
            {
                ServerPortNum = Convert.ToInt32(TBWorldServerPort.Text);
            }
            catch
            {
                TBWorldServerPort.Text = Convert.ToString(ServerPort);
                LauncherNotifier.Error($"ServerPort input should only contain numbers.");
                return;
            }
            
            // Limit the input to numbers in between ports 1024 and 65535
            if (ServerPortNum < 1024 || ServerPortNum > 65535)
            {
                TBWorldServerPort.Text = Convert.ToString(ServerPort);
                LauncherNotifier.Error($"Only port numbers between 1024 and 65535 are allowed.");
                return;
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

                //WorldManager.SelectedWorldName = SelectedWorldName;

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

        public string VersionToString(Version version)
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

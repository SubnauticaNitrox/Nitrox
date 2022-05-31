using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NitroxLauncher.Models;
using NitroxModel.DataStructures.GameLogic;
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

        // World settings variables
        public string SelectedWorldName { get; set; }
        public string SelectedWorldSeed { get; set; }
        public string SelectedWorldVersion { get; set; }
        public ServerGameMode SelectedWorldGamemode { get; set; }
        // Game Options
        public bool EnableCheatsValue { get; set; }
        public int MaxPlayerCap { get; set; }
        public Perms DefaultPlayerPermsValue { get; set; }
        // Server Options
        public bool EnableFullEntityCacheValue { get; set; }
        public bool EnableAutosaveValue { get; set; }
        public bool EnableJoinPasswordValue { get; set; }
        public bool EnableAutoPortForwardValue { get; set; }
        public int SaveInterval { get; set; }
        public string JoinPassword { get; set; }
        public int ServerPort { get; set; }
        public bool EnableLanDiscoveryValue { get; set; }

        public bool IsNewWorld { get; set; }
        private bool IsInSettings { get; set; }

        public string SelectedWorldDirectory { get; set; }
        
        public ServerPage()
        {
            InitializeComponent();
            InitializeWorldListing();

            RBIsDocked.IsChecked = !IsServerExternal;
            RBIsExternal.IsChecked = IsServerExternal;
        }

        private void RBServer_Clicked(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.IsExternalServer = RBIsExternal.IsChecked ?? true;
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
                else if (RBHardcore.IsChecked == true) { c.GameMode = ServerGameMode.HARDCORE; }

                c.DisableConsole = !EnableCheatsValue;
                c.MaxConnections = MaxPlayerCap;
                if (CBBDefaultPerms.SelectedIndex == 0) { c.DefaultPlayerPerm = Perms.PLAYER; }
                else if (CBBDefaultPerms.SelectedIndex == 1) { c.DefaultPlayerPerm = Perms.MODERATOR; }
                else if (CBBDefaultPerms.SelectedIndex == 2) { c.DefaultPlayerPerm = Perms.ADMIN; }

                c.CreateFullEntityCache = EnableFullEntityCacheValue;
                c.DisableAutoSave = !EnableAutosaveValue;
                //c.IsPasswordRequired = EnableJoinPasswordValue;
                c.AutoPortForward = EnableAutoPortForwardValue;
                c.SaveInterval = SaveInterval*1000;  // Convert seconds to milliseconds
                c.ServerPassword = JoinPassword;
                c.ServerPort = ServerPort;
                c.LANDiscoveryEnabled = EnableLanDiscoveryValue;

            });
            Log.Info($"Server Config updated");
        }

        public void UpdateVisualWorldSettings()
        {
            ServerConfig serverConfig = ServerConfig.Load(SelectedWorldDirectory);

            // Get config file values
            SelectedWorldName = Path.GetFileName(SelectedWorldDirectory);
            SelectedWorldSeed = serverConfig.Seed;
            SelectedWorldGamemode = serverConfig.GameMode;

            EnableCheatsValue = !serverConfig.DisableConsole;
            MaxPlayerCap = serverConfig.MaxConnections;
            DefaultPlayerPermsValue = serverConfig.DefaultPlayerPerm;

            EnableFullEntityCacheValue = serverConfig.CreateFullEntityCache;
            EnableAutosaveValue = !serverConfig.DisableAutoSave;
            EnableJoinPasswordValue = serverConfig.IsPasswordRequired;
            EnableAutoPortForwardValue = serverConfig.AutoPortForward;
            SaveInterval = serverConfig.SaveInterval/1000; // Convert milliseconds to seconds
            JoinPassword = serverConfig.ServerPassword;
            ServerPort = serverConfig.ServerPort;
            EnableLanDiscoveryValue = serverConfig.LANDiscoveryEnabled;

            // Set the world settings values to the server.cfg values
            TBWorldName.Text = SelectedWorldName;
            TBWorldSeed.Text = SelectedWorldSeed;
            if (SelectedWorldGamemode == ServerGameMode.FREEDOM) { RBFreedom.IsChecked = true; }
            else if (SelectedWorldGamemode == ServerGameMode.SURVIVAL) { RBSurvival.IsChecked = true; }
            else if (SelectedWorldGamemode == ServerGameMode.CREATIVE) { RBCreative.IsChecked = true; }
            else if (SelectedWorldGamemode == ServerGameMode.HARDCORE) { RBHardcore.IsChecked = true; }

            CBCheats.IsChecked = EnableCheatsValue;
            TBMaxPlayerCap.Text = Convert.ToString(MaxPlayerCap);
            if (DefaultPlayerPermsValue == Perms.PLAYER) { CBBDefaultPerms.SelectedIndex = 0; }
            if (DefaultPlayerPermsValue == Perms.MODERATOR) { CBBDefaultPerms.SelectedIndex = 1; }
            if (DefaultPlayerPermsValue == Perms.ADMIN) { CBBDefaultPerms.SelectedIndex = 2; }

            CBCreateFullEntityCache.IsChecked = EnableFullEntityCacheValue;
            CBAutoSave.IsChecked = EnableAutosaveValue;
            CBEnableJoinPassword.IsChecked = EnableJoinPasswordValue;
            CBAutoPortForward.IsChecked = EnableAutoPortForwardValue;
            TBSaveInterval.Text = Convert.ToString(SaveInterval);
            TBJoinPassword.Text = JoinPassword;
            if (string.IsNullOrEmpty(JoinPassword)) { TBJoinPassword.IsEnabled = false; JoinPasswordTitle.Opacity = .6; }
            TBWorldServerPort.Text = Convert.ToString(ServerPort);
            CBLanDiscovery.IsChecked = EnableLanDiscoveryValue;
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

        private void RefreshListing_Click(object sender, RoutedEventArgs e)
        {
            InitializeWorldListing();
        }
        
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            IsNewWorld = false;
            IsInSettings = false;
            SaveConfigSettings();
            InitializeWorldListing();

            Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
            GoBackAnimationStoryboard.Begin();
        }

        // World management
        private void SelectWorld_Click(object sender, RoutedEventArgs e)
        {
            if (WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex).IsValidSave)
            {
                TBWorldSeed.IsEnabled = false;

                Log.Info($"World index {WorldListingContainer.SelectedIndex} selected");

                SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";

                UpdateVisualWorldSettings();

                Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
                WorldSelectedAnimationStoryboard.Begin();
            }
            else
            {
                LauncherNotifier.Error($"This save is not a valid version.");
            }
        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == DeleteWorldBtn)
            {
                IsInSettings = true;
            }

            ConfirmationBox.Opacity = 1;
            ConfirmationBox.IsHitTestVisible = true;
        }

        private void YesConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsNewWorld)
            {
                SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";
            }
            IsNewWorld = false;

            Directory.Delete(SelectedWorldDirectory, true);
            Log.Info($"Deleting world \"{SelectedWorldName}\"");

            ConfirmationBox.Opacity = 0;
            ConfirmationBox.IsHitTestVisible = false;

            if (IsInSettings)
            {
                Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
                GoBackAnimationStoryboard.Begin();
            }
            IsInSettings = false;

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

        // Game Options
        private void CBCheats_Clicked(object sender, RoutedEventArgs e)
        {
            EnableCheatsValue = (bool)CBCheats.IsChecked;
        }

        private void TBMaxPlayerCap_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBMaxPlayerCap.Text = TBMaxPlayerCap.Text.TrimStart();
            TBMaxPlayerCap.Text = TBMaxPlayerCap.Text.TrimEnd();

            // Validation...
            if (string.IsNullOrEmpty(TBMaxPlayerCap.Text))
            {
                TBMaxPlayerCap.Text = Convert.ToString(MaxPlayerCap);
                LauncherNotifier.Error($"An empty Max Player Cap value is not valid.");
                return;
            }

            int MaxPlayerCapNum;
            try
            {
                MaxPlayerCapNum = Convert.ToInt32(TBMaxPlayerCap.Text);
            }
            catch
            {
                TBMaxPlayerCap.Text = Convert.ToString(MaxPlayerCap);
                LauncherNotifier.Error($"Max Player Cap input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 0
            if (MaxPlayerCapNum <= 0)
            {
                TBMaxPlayerCap.Text = Convert.ToString(MaxPlayerCap);
                LauncherNotifier.Error($"The Max Player Cap value cannot be zero or negative.");
                return;
            }

            MaxPlayerCap = Convert.ToInt32(TBMaxPlayerCap.Text);
        }

        // Server Options
        private void CBCreateFullEntityCache_Clicked(object sender, RoutedEventArgs e)
        {
            EnableFullEntityCacheValue = (bool)CBCreateFullEntityCache.IsChecked;
        }

        private void CBAutoSave_Clicked(object sender, RoutedEventArgs e)
        {
            EnableAutosaveValue = (bool)CBAutoSave.IsChecked;
        }

        private void CBEnableJoinPassword_Clicked(object sender, RoutedEventArgs e)
        {
            if ((bool)CBEnableJoinPassword.IsChecked)
            {
                TBJoinPassword.Opacity = 1;
                JoinPasswordTitle.Opacity = 1;
                TBJoinPassword.IsEnabled = true;
                Keyboard.Focus(TBJoinPassword);
            }
            else
            {
                TBJoinPassword.Opacity = .7;
                JoinPasswordTitle.Opacity = .6;
                TBJoinPassword.IsEnabled = false;
                JoinPassword = string.Empty;
            }
        }

        private void CBAutoPortForward_Clicked(object sender, RoutedEventArgs e)
        {
            EnableAutoPortForwardValue = (bool)CBAutoPortForward.IsChecked;
        }

        private void TBSaveInterval_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBSaveInterval.Text = TBSaveInterval.Text.TrimStart();
            TBSaveInterval.Text = TBSaveInterval.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBSaveInterval.Text))
            {
                TBSaveInterval.Text = Convert.ToString(SaveInterval);
                LauncherNotifier.Error($"An empty Save Interval value is not valid.");
                return;
            }

            int SaveIntervalNum;
            try
            {
                SaveIntervalNum = Convert.ToInt32(TBSaveInterval.Text);
            }
            catch
            {
                TBSaveInterval.Text = Convert.ToString(SaveInterval);
                LauncherNotifier.Error($"Save Interval input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 1
            if (SaveIntervalNum < 1)
            {
                TBSaveInterval.Text = Convert.ToString(SaveInterval);
                LauncherNotifier.Error($"The Save Interval value must be greater than 1.");
                return;
            }

            SaveInterval = Convert.ToInt32(TBSaveInterval.Text);
        }

        private void TBJoinPassword_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBJoinPassword.Text = TBJoinPassword.Text.TrimStart();
            TBJoinPassword.Text = TBJoinPassword.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBJoinPassword.Text))
            {
                CBEnableJoinPassword.IsChecked = false;
                TBJoinPassword.IsEnabled = false;
                JoinPasswordTitle.Opacity = .6;
            }

            JoinPassword = TBJoinPassword.Text;
        }

        private void TBWorldServerPort_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimStart();
            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBWorldServerPort.Text))
            {
                TBWorldServerPort.Text = Convert.ToString(ServerPort);
                LauncherNotifier.Error($"An empty Server Port value is not valid.");
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
                LauncherNotifier.Error($"Server Port input should only contain numbers.");
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

        private void CBLanDiscovery_Clicked(object sender, RoutedEventArgs e)
        {
            EnableLanDiscoveryValue = (bool)CBLanDiscovery.IsChecked;
        }


        // TODO
        //private void ViewModsPlugins_Click(object sender, RoutedEventArgs e)
        //{
        //    // Redirect user to the "Mods" tab of the launcher (for future reference if mod support is added) so that they can enable/disable mods
        //}

        // Start server button management
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";

            if (WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex).IsValidSave)
            {
                InitializeWorldListing();

                try
                {
                    LauncherLogic.Server.StartServer(RBIsExternal.IsChecked == true, SelectedWorldDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                LauncherNotifier.Error($"This save is not a valid version.");
            }

        }

        // Restore Backup Button(WIP)
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {

        }


        public string VersionToString(Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

    }
    // Uncomment to view intellisense world listings, in addition to the commented out lines that are in the ListView in ServerPage.xaml
    public class World_Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode { get; set; }
        public string WorldVersion { get; set; }
        public string WorldSaveDir { get; set; }
        public bool IsValidSave { get; set; }
    }
}

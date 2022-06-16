using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxLauncher.Models;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;
using Microsoft.VisualBasic.FileIO;

namespace NitroxLauncher.Pages
{
    public partial class ServerPage : PageBase
    {
        public bool IsServerExternal
        {
            get => LauncherLogic.Config.IsExternalServer;
            set => LauncherLogic.Config.IsExternalServer = value;
        }

        public ServerConfig Config;

        private bool IsNewWorld { get; set; }
        private bool IsInSettings { get; set; }

        private string SelectedWorldDirectory { get; set; }
        private string WorldCurrentlyUsed { get; set; }
        private string ImportedWorldName { get; set; }
        private string SelectedWorldImportDirectory { get; set; }
        private string SelectedServerCfgImportDirectory { get; set; }

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
            string dest = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), TBWorldName.Text);
            if (SelectedWorldDirectory != dest)
            {
                Directory.Move(SelectedWorldDirectory, dest+" temp"); // These two lines are needed to handle names that change in capitalization,
                Directory.Move(dest+" temp", dest);                   // since Windows still thinks of the two names as the same.
                SelectedWorldDirectory = dest;
            }

            Config = ServerConfig.Load(SelectedWorldDirectory);
            Config.Update(SelectedWorldDirectory, c =>
            {
                c.SaveName = TBWorldName.Text;
                if (IsNewWorld) { c.Seed = TBWorldSeed.Text; }
                if (RBFreedom.IsChecked == true) { c.GameMode = ServerGameMode.FREEDOM; }
                else if (RBSurvival.IsChecked == true) { c.GameMode = ServerGameMode.SURVIVAL; }
                else if (RBCreative.IsChecked == true) { c.GameMode = ServerGameMode.CREATIVE; }
                else if (RBHardcore.IsChecked == true) { c.GameMode = ServerGameMode.HARDCORE; }

                c.DisableConsole = !(bool)CBCheats.IsChecked;
                c.MaxConnections = Convert.ToInt32(TBMaxPlayerCap.Text);
                if (CBBDefaultPerms.SelectedIndex == 0) { c.DefaultPlayerPerm = Perms.PLAYER; }
                else if (CBBDefaultPerms.SelectedIndex == 1) { c.DefaultPlayerPerm = Perms.MODERATOR; }
                else if (CBBDefaultPerms.SelectedIndex == 2) { c.DefaultPlayerPerm = Perms.ADMIN; }

                c.CreateFullEntityCache = (bool)CBCreateFullEntityCache.IsChecked;
                c.DisableAutoSave = !(bool)CBAutoSave.IsChecked;
                c.AutoPortForward = (bool)CBAutoPortForward.IsChecked;
                c.SaveInterval = Convert.ToInt32(TBSaveInterval.Text)*1000;  // Convert seconds to milliseconds
                if ((bool)CBEnableJoinPassword.IsChecked) { c.ServerPassword = TBJoinPassword.Text; }
                else { c.ServerPassword = string.Empty; }
                c.ServerPort = Convert.ToInt32(TBWorldServerPort.Text);
                c.LANDiscoveryEnabled = (bool)CBLanDiscovery.IsChecked;
            });

        }

        public void UpdateVisualWorldSettings()
        {
            Config = ServerConfig.Load(SelectedWorldDirectory);

            // Set the world settings values to the server.cfg values
            TBWorldName.Text = Path.GetFileName(SelectedWorldDirectory);
            TBWorldSeed.Text = Config.Seed;
            if (Config.GameMode == ServerGameMode.FREEDOM) { RBFreedom.IsChecked = true; }
            else if (Config.GameMode == ServerGameMode.SURVIVAL) { RBSurvival.IsChecked = true; }
            else if (Config.GameMode == ServerGameMode.CREATIVE) { RBCreative.IsChecked = true; }
            else if (Config.GameMode == ServerGameMode.HARDCORE) { RBHardcore.IsChecked = true; }

            CBCheats.IsChecked = !Config.DisableConsole;
            TBMaxPlayerCap.Text = Convert.ToString(Config.MaxConnections);
            if (Config.DefaultPlayerPerm == Perms.PLAYER) { CBBDefaultPerms.SelectedIndex = 0; }
            if (Config.DefaultPlayerPerm == Perms.MODERATOR) { CBBDefaultPerms.SelectedIndex = 1; }
            if (Config.DefaultPlayerPerm == Perms.ADMIN) { CBBDefaultPerms.SelectedIndex = 2; }

            CBCreateFullEntityCache.IsChecked = Config.CreateFullEntityCache;
            CBAutoSave.IsChecked = !Config.DisableAutoSave;
            CBEnableJoinPassword.IsChecked = Config.IsPasswordRequired;
            CBAutoPortForward.IsChecked = Config.AutoPortForward;
            TBSaveInterval.Text = Convert.ToString(Config.SaveInterval/1000); // Convert milliseconds to seconds
            TBJoinPassword.Text = Config.ServerPassword;
            if (string.IsNullOrEmpty(Config.ServerPassword)) { TBJoinPassword.IsEnabled = false; JoinPasswordTitle.Opacity = .6; }
            TBWorldServerPort.Text = Convert.ToString(Config.ServerPort);
            CBLanDiscovery.IsChecked = Config.LANDiscoveryEnabled;

            if (Config.IsPasswordRequired)
            {
                TBJoinPassword.Opacity = 1;
                JoinPasswordTitle.Opacity = 1;
                TBJoinPassword.IsEnabled = true;
            }

        }

        // Pane Buttons
        public void AddWorld_Click(object sender, RoutedEventArgs e)
        {
            Log.Info($"Adding new world");
            IsNewWorld = true;
            TBWorldSeed.IsEnabled = true;

            ImportSaveBtnBorder.Opacity = 1;
            ImportSaveBtn.IsEnabled = true;

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
            if (!Directory.Exists(SelectedWorldDirectory))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
            }
            else
            {
                SaveConfigSettings();
                InitializeWorldListing();
                IsNewWorld = false;
                IsInSettings = false;

                ImportSaveBtnBorder.Opacity = 0;
                ImportSaveBtn.IsEnabled = false;
            }

            Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
            GoBackAnimationStoryboard.Begin();
        }

        private void ImportSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ImportedWorldName = string.Empty;
            SelectedWorldImportDirectory = string.Empty;
            SelectedServerCfgImportDirectory = string.Empty;
            TBImportedWorldName.Text = string.Empty;
            TBSelectedSaveImportDir.Text = "Select the save file to import";
            TBSelectedServerCfgImportDir.Text = "Select the server.cfg file to import";

            ImportWorldBox.Opacity = 1;
            ImportWorldBox.IsHitTestVisible = true;
            SelectImportedServerCfgBtn.IsEnabled = false;
            SelectImportedServerCfgBtn.Opacity = .6;
            ImportWorldBtn.IsEnabled = false;
            ImportWorldBtn.Opacity = .6;
        }

        // World management
        private void SelectedWorldSettings_Click(object sender, RoutedEventArgs e)
        {
            if (LauncherLogic.Server.IsServerRunning && WorldCurrentlyUsed == WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir)
            {
                LauncherNotifier.Error("This world is currently being used. Stop the server to edit the settings of this world");
                return;
            }
            else if (!Directory.Exists(SelectedWorldDirectory))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
                return;
            }
            else if (!WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex).IsValidSave)
            {
                LauncherNotifier.Error($"This save is an invalid version.");
                return;
            }

            TBWorldSeed.IsEnabled = false;

            SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";

            UpdateVisualWorldSettings();

            Storyboard WorldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            WorldSelectedAnimationStoryboard.Begin();
        }

        // Restore Backup Button(WIP)
        //private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {
            if (LauncherLogic.Server.IsServerRunning && WorldCurrentlyUsed == WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir)
            {
                LauncherNotifier.Error("This world is currently being used. Stop the server to delete this world");
                return;
            }
            else if (!Directory.Exists(SelectedWorldDirectory))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
                if (e.OriginalSource == DeleteWorldBtn)
                {
                    Storyboard GoBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
                    GoBackAnimationStoryboard.Begin();
                }
                return;
            }

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
            
            try
            {
                FileSystem.DeleteDirectory(SelectedWorldDirectory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                Log.Info($"Moving world \"{Path.GetFileName(SelectedWorldDirectory)}\" to the recyling bin.");
                LauncherNotifier.Success($"Successfully moved save \"{Path.GetFileName(SelectedWorldDirectory)}\" to the recycling bin");
            }
            catch (Exception ex)
            {
                LauncherNotifier.Error("Error: Could not move the selected save to the recycling bin. Try deleting any remaining files manually.");
                Log.Error($"Could not move save \"{Path.GetFileName(SelectedWorldDirectory)}\" to the recycling bin : {ex.GetType()} {ex.Message}");
            }

            ConfirmationBox.Opacity = 0;
            ConfirmationBox.IsHitTestVisible = false;

            if (IsInSettings)
            {
                IsNewWorld = false;
                ImportSaveBtnBorder.Opacity = 0;
                ImportSaveBtn.IsEnabled = false;

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

        // World settings management
        private void TBWorldName_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string originalName = Path.GetFileName(SelectedWorldDirectory);

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
                    if (WorldManager.ValidateSave(newSelectedWorldDirectory))
                    {
                        LauncherNotifier.Error($"World name \"{TBWorldName.Text}\" already exists.");
                    }
                    else
                    {
                        LauncherNotifier.Error($"A folder named \"{TBWorldName.Text}\" already exists in the saves folder.");
                    }

                    int i = 1;
                    Regex rx = new(@"\((\d+)\)$");
                    if (!rx.IsMatch(TBWorldName.Text))
                    {
                        originalName = TBWorldName.Text + $" ({i})";
                        newSelectedWorldDirectory = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), originalName);
                    }

                    while (Directory.Exists(newSelectedWorldDirectory) && !newSelectedWorldDirectory.Equals(SelectedWorldDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        // Increment the number to the end of the name until it reaches an available filename
                        originalName = rx.Replace(originalName, $"({i})", 1);
                        newSelectedWorldDirectory = Path.Combine(Path.GetDirectoryName(SelectedWorldDirectory) ?? throw new Exception("Selected world is empty"), originalName);
                        i++;
                    }

                    TBWorldName.Text = originalName;
                }

            }
            else
            {
                TBWorldName.Text = originalName;
                LauncherNotifier.Error($"An empty world name is not valid.");
            }
        }

        private void TBWorldSeed_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (TBWorldSeed.Text.Length != 0)
            {
                string originalSeed = Config.Seed;

                TBWorldSeed.Text = TBWorldSeed.Text.TrimStart();
                TBWorldSeed.Text = TBWorldSeed.Text.TrimEnd();
                TBWorldSeed.Text = TBWorldSeed.Text.ToUpper();

                if (TBWorldSeed.Text.Length != 10 || !Regex.IsMatch(TBWorldSeed.Text, @"^[a-zA-Z]+$"))
                {
                    TBWorldSeed.Text = originalSeed;
                    LauncherNotifier.Error($"World Seeds should contain 10 alphabetical characters (A-Z).");
                    return;
                }

            }
        }

        private void TBMaxPlayerCap_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string originalMaxPlayerCap = Convert.ToString(Config.MaxConnections);

            TBMaxPlayerCap.Text = TBMaxPlayerCap.Text.TrimStart();
            TBMaxPlayerCap.Text = TBMaxPlayerCap.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBMaxPlayerCap.Text))
            {
                TBMaxPlayerCap.Text = originalMaxPlayerCap;
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
                TBMaxPlayerCap.Text = originalMaxPlayerCap;
                LauncherNotifier.Error($"Max Player Cap input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 0
            if (MaxPlayerCapNum <= 0)
            {
                TBMaxPlayerCap.Text = originalMaxPlayerCap;
                LauncherNotifier.Error($"The Max Player Cap value cannot be zero or negative.");
                return;
            }

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
            }
        }
        
        private void TBSaveInterval_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string originalSaveInterval = Convert.ToString(Config.SaveInterval);

            TBSaveInterval.Text = TBSaveInterval.Text.TrimStart();
            TBSaveInterval.Text = TBSaveInterval.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBSaveInterval.Text))
            {
                TBSaveInterval.Text = originalSaveInterval;
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
                TBSaveInterval.Text = originalSaveInterval;
                LauncherNotifier.Error($"Save Interval input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 1
            if (SaveIntervalNum < 1)
            {
                TBSaveInterval.Text = originalSaveInterval;
                LauncherNotifier.Error($"The Save Interval value must be greater than 1.");
                return;
            }

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

        }

        private void TBWorldServerPort_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string originalServerPort = Convert.ToString(Config.ServerPort);

            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimStart();
            TBWorldServerPort.Text = TBWorldServerPort.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBWorldServerPort.Text))
            {
                TBWorldServerPort.Text = originalServerPort;
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
                TBWorldServerPort.Text = originalServerPort;
                LauncherNotifier.Error($"Server Port input should only contain numbers.");
                return;
            }
            
            // Limit the input to numbers in between ports 1024 and 65535
            if (ServerPortNum < 1024 || ServerPortNum > 65535)
            {
                TBWorldServerPort.Text = originalServerPort;
                LauncherNotifier.Error($"Only port numbers between 1024 and 65535 are allowed.");
                return;
            }

        }

        // TODO
        //private void ViewModsPlugins_Click(object sender, RoutedEventArgs e)
        //{
        //    // Redirect user to the "Mods" tab of the launcher (for future reference if mod support is added) so that they can enable/disable mods
        //}

        // Save File Import
        private void TBImportedWorldName_Input(object sender, KeyboardFocusChangedEventArgs e)
        {
            TBImportedWorldName.Text = TBImportedWorldName.Text.TrimStart();
            TBImportedWorldName.Text = TBImportedWorldName.Text.TrimEnd();

            // Make sure the string isn't empty
            if (!string.IsNullOrEmpty(TBImportedWorldName.Text))
            {
                // Make sure the world name is valid as a file name
                bool isIllegalName = false;
                char[] illegalChars = Path.GetInvalidFileNameChars();
                for (int i = 0; !isIllegalName && i < illegalChars.Length; i++)
                {
                    isIllegalName = TBImportedWorldName.Text.Contains(illegalChars[i]);
                }

                if (isIllegalName)
                {
                    TBImportedWorldName.Text = string.Join("_", TBImportedWorldName.Text.Split(Path.GetInvalidFileNameChars()));
                    LauncherNotifier.Error("World names cannot contain these characters: < > : \" / \\ | ? *");
                }

                // Check that name is not a duplicate if it was changed
                string newSelectedWorldDirectory = Path.Combine(WorldManager.SavesFolderDir, TBImportedWorldName.Text);
                if (!TBImportedWorldName.Text.Equals(ImportedWorldName, StringComparison.OrdinalIgnoreCase) && Directory.Exists(newSelectedWorldDirectory))
                {
                    if (WorldManager.ValidateSave(newSelectedWorldDirectory))
                    {
                        LauncherNotifier.Error($"World name \"{TBImportedWorldName.Text}\" already exists.");
                    }
                    else
                    {
                        LauncherNotifier.Error($"A folder named \"{TBImportedWorldName.Text}\" already exists in the saves folder.");
                    }

                    int i = 1;
                    Regex rx = new(@"\((\d+)\)$");
                    if (!rx.IsMatch(TBImportedWorldName.Text))
                    {
                        ImportedWorldName = TBImportedWorldName.Text + $" ({i})";
                        newSelectedWorldDirectory = Path.Combine(WorldManager.SavesFolderDir, ImportedWorldName);
                    }

                    // If save name still exists, increment the number to the end of the name until it reaches an available filename
                    while (Directory.Exists(newSelectedWorldDirectory))
                    {
                        ImportedWorldName = rx.Replace(ImportedWorldName, $"({i})", 1);
                        newSelectedWorldDirectory = Path.Combine(WorldManager.SavesFolderDir, ImportedWorldName);
                        i++;
                    }

                    TBImportedWorldName.Text = ImportedWorldName;
                }
                
                ImportedWorldName = TBImportedWorldName.Text;

                // Enable the "Import" button if user has selected a save file and a server.cfg file
                if (!string.IsNullOrEmpty(SelectedWorldImportDirectory) && !string.IsNullOrEmpty(SelectedServerCfgImportDirectory)/*TBSelectedServerCfgImportDir.Text != "Select the server.cfg file to import"*/)
                {
                    ImportWorldBtn.IsEnabled = true;
                    ImportWorldBtn.Opacity = 1;
                }

            }
            else
            {
                ImportWorldBtn.IsEnabled = false;
                ImportWorldBtn.Opacity = .6;
            }
        }

        private void SelectImportedSaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new()
            {
                Multiselect = false,
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                EnsurePathExists = true,
                IsFolderPicker = true,
                Title = "Select the save file to import"
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                SelectedWorldImportDirectory = Path.GetFullPath(dialog.FileName);
            }
            if (Convert.ToString(Directory.GetParent(SelectedWorldImportDirectory)) == WorldManager.SavesFolderDir)
            {
                LauncherNotifier.Error("There is no point in importing a save file you already have in the saves directory. Please select a different save file to import.");
                return;
            }
            if (!WorldManager.ValidateSave(SelectedWorldImportDirectory))
            {
                // Give special warning if game save file is selected
                if (File.Exists(Path.Combine(SelectedWorldImportDirectory, "gameinfo.json")) && File.Exists(Path.Combine(SelectedWorldImportDirectory, "global-objects.bin")) && File.Exists(Path.Combine(SelectedWorldImportDirectory, "scene-objects.bin")))
                {
                    LauncherNotifier.Error("Singleplayer saves cannot be imported and used by Nitrox: Save formats are incompatible.");
                }
                else if (File.Exists(Path.Combine(SelectedWorldImportDirectory, "WorldData.nitrox")))
                {
                    LauncherNotifier.Error("Protobuf saves are no longer supported and cannot be imported. Please run the \"swapserializer json\" command on server of the previous Nitrox version you used to change it to JSON.");
                }
                else
                {
                    LauncherNotifier.Error("Invalid save file selected.");
                }
                return;
            }

            TBSelectedSaveImportDir.Text = SelectedWorldImportDirectory;

            // Check if the selected save file has a server.cfg file already inside of it, and set that path in the server.cfg panel. If not, clear the server.cfg panel
            if (File.Exists(Path.Combine(SelectedWorldImportDirectory, Config.FileName)))
            {
                SelectedServerCfgImportDirectory = Path.Combine(SelectedWorldImportDirectory, Config.FileName);
                TBSelectedServerCfgImportDir.Text = SelectedServerCfgImportDirectory;

                LauncherNotifier.Info("A server.cfg file was detected inside the selected save file");
            }
            else
            {
                SelectedServerCfgImportDirectory = string.Empty;
                TBSelectedServerCfgImportDir.Text = "Select the server.cfg file to import";
            }

            // Enable the "Import" button if user has set a save name and selected a server.cfg file
            if (!string.IsNullOrEmpty(ImportedWorldName) && !string.IsNullOrEmpty(SelectedServerCfgImportDirectory))
            {
                ImportWorldBtn.IsEnabled = true;
                ImportWorldBtn.Opacity = 1;
            }
            SelectImportedServerCfgBtn.IsEnabled = true;
            SelectImportedServerCfgBtn.Opacity = 1;

        }

        private void SelectImportedServerCfgBtn_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new()
            {
                Multiselect = false,
                EnsurePathExists = true,
                Title = "Select the server.cfg file to import",
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                SelectedServerCfgImportDirectory = Path.GetFullPath(dialog.FileName);
            }
            if (Path.GetFileName(SelectedServerCfgImportDirectory) != "server.cfg")
            {
                LauncherNotifier.Error("Invalid file selected. Please select a valid server.cfg file");
                return;
            }

            TBSelectedServerCfgImportDir.Text = SelectedServerCfgImportDirectory;

            // Enable the "Import" button if user has set a save name and selected a save file
            if (!string.IsNullOrEmpty(ImportedWorldName) && !string.IsNullOrEmpty(SelectedWorldImportDirectory))
            {
                ImportWorldBtn.IsEnabled = true;
                ImportWorldBtn.Opacity = 1;
            }
        }

        private void ImportWorldBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SelectedWorldDirectory))
            {
                try
                {
                    FileSystem.DeleteDirectory(SelectedWorldDirectory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    LauncherNotifier.Info("Could not delete the originally created world. You can manually delete this world separately if desired.");
                    Log.Error($"Could not move save \"{Path.GetFileName(SelectedWorldDirectory)}\" to the recycling bin : {ex.GetType()} {ex.Message}");
                }
            }
            SelectedWorldDirectory = Path.Combine(WorldManager.SavesFolderDir, ImportedWorldName);

            // Create save folder
            Directory.CreateDirectory(SelectedWorldDirectory);

            // Copy over targeted server.cfg file and ensure its serializer is set to JSON to prevent future errors
            File.Copy(SelectedServerCfgImportDirectory, Path.Combine(SelectedWorldDirectory, "server.cfg"));
            ServerConfig importedServerConfig = ServerConfig.Load(Path.Combine(SelectedWorldDirectory));
            if (importedServerConfig.SerializerMode != ServerSerializerMode.JSON)
            {
                importedServerConfig.Update(SelectedWorldDirectory, c =>
                {
                    c.SerializerMode = ServerSerializerMode.JSON;
                });
            }

            // Copy over specific save files from within the targeted folder
            foreach (string file in WorldManager.WorldFiles)
            {
                string targetFileDir = Path.Combine(SelectedWorldDirectory, file);
                File.Copy(Path.Combine(SelectedWorldImportDirectory, file), targetFileDir);
                File.SetLastWriteTime(targetFileDir, DateTime.Now);
            }

            UpdateVisualWorldSettings();

            ImportSaveBtnBorder.Opacity = 0;
            ImportSaveBtn.IsEnabled = false;
            ImportWorldBox.Opacity = 0;
            ImportWorldBox.IsHitTestVisible = false;
            TBWorldSeed.IsEnabled = false;

            LauncherNotifier.Success("Successfully imported the selected save file");
        }

        private void ImportWorldCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ImportWorldBox.Opacity = 0;
            ImportWorldBox.IsHitTestVisible = false;
        }

        // Start Server button
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorldDirectory = WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex)?.WorldSaveDir ?? "";

            if (!WorldManager.GetSaves().ElementAtOrDefault(WorldListingContainer.SelectedIndex).IsValidSave)
            {
                LauncherNotifier.Error($"This save is an invalid version.");
                return;
            }
            else if (!Directory.Exists(SelectedWorldDirectory))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
                return;
            }

            try
            {
                LauncherLogic.Server.StartServer(RBIsExternal.IsChecked == true, SelectedWorldDirectory);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("An instance of Nitrox Server is already running"))
                {
                    LauncherNotifier.Error("An instance of the Nitrox server is already running, please close it to start another server.");
                }
                else
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            WorldCurrentlyUsed = SelectedWorldDirectory;
            File.SetLastWriteTime(Path.Combine(SelectedWorldDirectory, "WorldData.json"), DateTime.Now);
            InitializeWorldListing();
        }

    }

    // OPTIONAL - Only used to view world listings in intellisense, in addition to the lines that are in the ListView definition in ServerPage.xaml
    public class World_Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode { get; set; }
        public string WorldVersion { get; set; }
        public bool IsValidSave { get; set; }
    }
}

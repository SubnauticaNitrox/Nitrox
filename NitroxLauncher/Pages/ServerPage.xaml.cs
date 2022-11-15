using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
        public ServerConfig Config;

        public bool IsNewWorld { get; private set; }
        private bool IsInSettings { get; set; }
        private string SelectedWorldDirectory { get; set; }
        private string WorldDirCurrentlyUsed { get; set; }
        private string ImportedWorldName { get; set; }
        private string SelectedWorldImportDirectory { get; set; }
        private string SelectedServerCfgImportDirectory { get; set; }
        private WorldManager.Listing SelectedListing { get; set; }

        public ServerPage()
        {
            InitializeComponent();

            InitializeWorldListing();
            RBIsDocked.IsChecked = !LauncherLogic.Config.IsExternalServer;
            RBIsExternal.IsChecked = LauncherLogic.Config.IsExternalServer;
        }

        private void RBServer_Clicked(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.IsExternalServer = RBIsExternal.IsChecked ?? true;
        }

        public void InitializeWorldListing()
        {
            WorldManager.Refresh();

            NoWorldsBackground.Opacity = WorldManager.GetSaves().Any() ? 0 : 1;

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

                c.DisableConsole = !CBCheats.IsChecked ?? c.DisableConsole;
                c.MaxConnections = Convert.ToInt32(TBMaxPlayerCap.Text);
                c.DefaultPlayerPerm = CBBDefaultPerms.SelectedIndex switch
                {
                    0 => Perms.PLAYER,
                    1 => Perms.MODERATOR,
                    2 => Perms.ADMIN,
                    _ => c.DefaultPlayerPerm
                };
                c.CreateFullEntityCache = CBCreateFullEntityCache.IsChecked ?? c.CreateFullEntityCache;
                c.DisableAutoSave = !CBAutoSave.IsChecked ?? c.DisableAutoSave;
                c.AutoPortForward = CBAutoPortForward.IsChecked ?? c.AutoPortForward;
                c.SaveInterval = Convert.ToInt32(TBSaveInterval.Text)*1000;  // Convert seconds to milliseconds
                if (CBEnableJoinPassword.IsChecked ?? false)
                {
                    c.ServerPassword = TBJoinPassword.Text;
                }
                else { c.ServerPassword = string.Empty; }
                c.ServerPort = Convert.ToInt32(TBWorldServerPort.Text);
                c.LANDiscoveryEnabled = CBLanDiscovery.IsChecked ?? c.LANDiscoveryEnabled;
            });

        }

        public void UpdateVisualWorldSettings()
        {
            Config = ServerConfig.Load(SelectedWorldDirectory);

            // Set the world settings values to the server.cfg values
            TBWorldName.Text = Path.GetFileName(SelectedWorldDirectory);
            TBWorldSeed.Text = Config.Seed;
            switch (Config.GameMode)
            {
                case ServerGameMode.FREEDOM:
                    RBFreedom.IsChecked = true;
                    break;
                case ServerGameMode.SURVIVAL:
                    RBSurvival.IsChecked = true;
                    break;
                case ServerGameMode.CREATIVE:
                    RBCreative.IsChecked = true;
                    break;
                case ServerGameMode.HARDCORE:
                    RBHardcore.IsChecked = true;
                    break;
            }
            CBCheats.IsChecked = !Config.DisableConsole;
            TBMaxPlayerCap.Text = Convert.ToString(Config.MaxConnections);
            CBBDefaultPerms.SelectedIndex = Config.DefaultPlayerPerm switch
            {
                Perms.PLAYER => 0,
                Perms.MODERATOR => 1,
                Perms.ADMIN => 2,
                _ => CBBDefaultPerms.SelectedIndex
            };
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
            TBWorldSeed.IsReadOnly = false;

            ImportSaveBtnBorder.Opacity = 1;
            ImportSaveBtn.IsEnabled = true;

            SelectedWorldDirectory = WorldManager.CreateEmptySave("My World");
            UpdateVisualWorldSettings();

            Storyboard worldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            worldSelectedAnimationStoryboard.Begin();

        }

        private void RefreshListing_Click(object sender, RoutedEventArgs e)
        {
            InitializeWorldListing();
        }
        
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            WorldManager.Listing selectedWorld = GetWorldListingFromSenderControl(sender);
            if (!IsNewWorld)
            {
                SelectedWorldDirectory = selectedWorld.WorldSaveDir;
            }

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

            ((Storyboard)FindResource("GoBackAnimation")).Begin();
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

        private void SelectedWorldSettings_Click(object sender, RoutedEventArgs e)
        {
            WorldManager.Listing selectedWorld = GetWorldListingFromSenderControl(sender);
            if (LauncherLogic.Server.IsServerRunning && WorldDirCurrentlyUsed == selectedWorld.WorldSaveDir)
            {
                LauncherNotifier.Error("This world is currently being used. Stop the server to edit the settings of this world");
                return;
            }
            if (!Directory.Exists(selectedWorld.WorldSaveDir))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
                return;
            }
            if (!selectedWorld.IsValidSave)
            {
                LauncherNotifier.Error($"This save is an invalid version.");
                return;
            }

            TBWorldSeed.IsReadOnly = true;

            SelectedWorldDirectory = selectedWorld.WorldSaveDir ?? "";

            UpdateVisualWorldSettings();

            Storyboard worldSelectedAnimationStoryboard = (Storyboard)FindResource("WorldSelectedAnimation");
            worldSelectedAnimationStoryboard.Begin();
        }

        // Restore Backup Button (TODO)
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeleteWorld_Click(object sender, RoutedEventArgs e)
        {
            WorldManager.Listing selectedWorld = GetWorldListingFromSenderControl(sender);
            if (!IsNewWorld)
            {
                SelectedWorldDirectory = selectedWorld.WorldSaveDir;
            }
            
            if (LauncherLogic.Server.IsServerRunning && WorldDirCurrentlyUsed == selectedWorld.WorldSaveDir)
            {
                LauncherNotifier.Error("This world is currently being used. Stop the server to delete this world");
                return;
            }
            if (!Directory.Exists(SelectedWorldDirectory))
            {
                LauncherNotifier.Error($"This save does not exist or is not valid.");
                InitializeWorldListing();
                return;
            }

            ConfirmationBox.Opacity = 1;
            ConfirmationBox.IsHitTestVisible = true;
        }

        private void YesConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            WorldManager.Listing selectedWorld = GetWorldListingFromSenderControl(sender);
            if (!IsNewWorld)
            {
                SelectedWorldDirectory = selectedWorld.WorldSaveDir ?? "";
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

                Storyboard goBackAnimationStoryboard = (Storyboard)FindResource("GoBackAnimation");
                goBackAnimationStoryboard.Begin();
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
        private void TBWorldName_Input(object sender, KeyboardFocusChangedEventArgs e)
        {
            string originalName = Path.GetFileName(SelectedWorldDirectory);

            TBWorldName.Text = TBWorldName.Text.TrimStart();
            TBWorldName.Text = TBWorldName.Text.TrimEnd();

            // Make sure the string isn't empty
            if (string.IsNullOrEmpty(TBWorldName.Text))
            {
                TBWorldName.Text = originalName;
                LauncherNotifier.Error($"An empty world name is not valid.");
                return;
            }
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
                return;
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

        private void TBWorldSeed_Input(object sender, KeyboardFocusChangedEventArgs e)
        {
            TBWorldSeed.Text = TBWorldSeed.Text.TrimStart();
            TBWorldSeed.Text = TBWorldSeed.Text.TrimEnd();

            if (!string.IsNullOrEmpty(TBWorldSeed.Text))
            {
                string originalSeed = Config.Seed;

                TBWorldSeed.Text = TBWorldSeed.Text.ToUpper();

                if (TBWorldSeed.Text.Length != 10 || !Regex.IsMatch(TBWorldSeed.Text, @"^[a-zA-Z]+$"))
                {
                    TBWorldSeed.Text = originalSeed;
                    LauncherNotifier.Error($"World Seeds should contain 10 alphabetical characters (A-Z).");
                }
            }

            Config.Seed = TBWorldSeed.Text;
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

            int maxPlayerCapNum;
            try
            {
                maxPlayerCapNum = Convert.ToInt32(TBMaxPlayerCap.Text);
            }
            catch
            {
                TBMaxPlayerCap.Text = originalMaxPlayerCap;
                LauncherNotifier.Error($"Max Player Cap input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 0
            if (maxPlayerCapNum <= 0)
            {
                TBMaxPlayerCap.Text = originalMaxPlayerCap;
                LauncherNotifier.Error($"The Max Player Cap value cannot be zero or negative.");
                return;
            }

            Config.MaxConnections = maxPlayerCapNum;
        }

        private void CBEnableJoinPassword_Clicked(object sender, RoutedEventArgs e)
        {
            if (CBEnableJoinPassword.IsChecked ?? false)
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
            string originalSaveInterval = Convert.ToString(Config.SaveInterval/1000);

            TBSaveInterval.Text = TBSaveInterval.Text.TrimStart();
            TBSaveInterval.Text = TBSaveInterval.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBSaveInterval.Text))
            {
                TBSaveInterval.Text = originalSaveInterval;
                LauncherNotifier.Error($"An empty Save Interval value is not valid.");
                return;
            }

            int saveIntervalNum;
            try
            {
                saveIntervalNum = Convert.ToInt32(TBSaveInterval.Text)*1000;
            }
            catch
            {
                TBSaveInterval.Text = originalSaveInterval;
                LauncherNotifier.Error($"Save Interval input should only contain numbers.");
                return;
            }

            // Limit save interval value to numbers greater than 1
            if (saveIntervalNum < 1)
            {
                TBSaveInterval.Text = originalSaveInterval;
                LauncherNotifier.Error($"The Save Interval value must be greater than 1.");
                return;
            }

            Config.SaveInterval = saveIntervalNum;
        }

        private void TBJoinPassword_Input(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TBJoinPassword.Text = TBJoinPassword.Text.TrimStart();
            TBJoinPassword.Text = TBJoinPassword.Text.TrimEnd();

            if (string.IsNullOrEmpty(TBJoinPassword.Text) && CBEnableJoinPassword.IsMouseOver == false)
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

            int serverPortNum;
            try
            {
                serverPortNum = Convert.ToInt32(TBWorldServerPort.Text);
            }
            catch
            {
                TBWorldServerPort.Text = originalServerPort;
                LauncherNotifier.Error($"Server Port input should only contain numbers.");
                return;
            }
            
            // Limit the input to numbers in between ports 1024 and 65535
            if (serverPortNum < 1024 || serverPortNum > 65535)
            {
                TBWorldServerPort.Text = originalServerPort;
                LauncherNotifier.Error($"Only port numbers between 1024 and 65535 are allowed.");
                return;
            }

            Config.ServerPort = serverPortNum;
        }

        // TODO: Redirect user to the "Mods/Plugins" tab of the launcher (for future reference if mod support is added) so that they can enable/disable mods
        private void ViewModsPlugins_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Save File Import
        private void TBImportedWorldName_Input(object sender, KeyboardFocusChangedEventArgs e) // UX TODO: Set this textbox to be the same as the original world name
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
                if (!string.IsNullOrEmpty(SelectedWorldImportDirectory) && !string.IsNullOrEmpty(SelectedServerCfgImportDirectory))
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
                else if (File.Exists(Path.Combine(SelectedWorldImportDirectory, "WorldData.nitrox")) && !File.Exists(Path.Combine(SelectedWorldImportDirectory, "WorldData.json")))
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

            try
            {
                // Create save folder
                Directory.CreateDirectory(SelectedWorldDirectory);

                // Copy over targeted server.cfg file and ensure its serializer is set to JSON to prevent future errors
                FileSystem.CopyFile(SelectedServerCfgImportDirectory, Path.Combine(SelectedWorldDirectory, "server.cfg"));
                ServerConfig importedServerConfig = ServerConfig.Load(Path.Combine(SelectedWorldDirectory));
                if (importedServerConfig.SerializerMode != ServerSerializerMode.JSON)
                {
                    importedServerConfig.Update(SelectedWorldDirectory, c =>
                    {
                        c.SerializerMode = ServerSerializerMode.JSON;
                    });
                }

                // Copy over specific save files from within the targeted folder
                foreach (string file in Directory.EnumerateFiles(SelectedWorldImportDirectory))
                {
                    string targetFileDir = Path.Combine(SelectedWorldImportDirectory, file);
                    string destFileDir = Path.Combine(SelectedWorldDirectory, Path.GetFileName(file));

                    if (Path.GetExtension(targetFileDir) != ".json")
                    {
                        continue;
                    }

                    FileSystem.CopyFile(targetFileDir, destFileDir);
                    File.SetLastWriteTime(destFileDir, DateTime.Now);
                }

                UpdateVisualWorldSettings();

                ImportSaveBtnBorder.Opacity = 0;
                ImportSaveBtn.IsEnabled = false;
                ImportWorldBox.Opacity = 0;
                ImportWorldBox.IsHitTestVisible = false;
                TBWorldSeed.IsReadOnly = true;

                LauncherNotifier.Success("Successfully imported the selected save file");
            }
            catch (Exception ex)
            {
                LauncherNotifier.Error("Failed to import the selected save file. Please check your log for details.");
                Log.Error($"Could not import save \"{Path.GetFileName(SelectedWorldDirectory)}\" : {ex.GetType()} {ex.Message}");
            }
        }

        private void ImportWorldCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ImportWorldBox.Opacity = 0;
            ImportWorldBox.IsHitTestVisible = false;
        }

        // Start Server button
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            WorldManager.Listing listing = GetWorldListingFromSenderControl(sender);
            try
            {
                SelectedWorldDirectory = listing.WorldSaveDir ?? "";

                if (!listing.IsValidSave) // UX TODO: Handle world selection before starting the server for better UX
                {
                    LauncherNotifier.Error($"This save is of an unsupported version of Nitrox.");
                    return;
                }
                if (!Directory.Exists(listing.WorldSaveDir))
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

                if (File.Exists(Path.Combine(SelectedWorldDirectory, "WorldData.json")))
                {
                    File.SetLastWriteTime(Path.Combine(SelectedWorldDirectory, "WorldData.json"), DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error while starting server", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            WorldDirCurrentlyUsed = SelectedWorldDirectory;
            InitializeWorldListing();
        }

        private WorldManager.Listing GetWorldListingFromSenderControl(object sender)
        {
            Control control = (Control)sender;
            return SelectedListing = control.DataContext as WorldManager.Listing ?? control.FindDataContextInAncestors<WorldManager.Listing>() ?? SelectedListing;
        }
    }

    [Serializable]
    internal class WorldListing
    {
        public string WorldName { get; set; }

        public string WorldGamemode { get; set; }

        public string WorldVersion { get; set; }

        public bool IsValidSave { get; set; }

        public override string ToString()
        {
            return $"[{nameof(WorldListing)}: WorldName: {WorldName}, WorldGamemode: {WorldGamemode}, WorldVersion: {WorldVersion}, IsValidSave: {IsValidSave}]";
        }
    }
}

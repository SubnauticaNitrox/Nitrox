using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxLauncher.Models;
using NitroxModel.Discovery;
using NitroxServer.Serialization.World;

namespace NitroxLauncher.Pages
{
    public partial class OptionPage : PageBase
    {
        public Platform GamePlatform => LauncherLogic.Config.SubnauticaPlatform;
        public string PathToSubnautica => LauncherLogic.Config.SubnauticaPath;
        public string SubnauticaLaunchArguments => LauncherLogic.Config.SubnauticaLaunchArguments;

        public OptionPage()
        {
            InitializeComponent();
            SaveFileLocationTextblock.Text = WorldManager.SavesFolderDir;

            ArgumentsTextbox.Text = SubnauticaLaunchArguments;
            if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
            {
                ResetButton.Visibility = Visibility.Visible;
            }

            Loaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
                OnLogicPropertyChanged(null, null);
            };

            Unloaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
            };
        }

        private async void OnChangePath_Click(object sender, RoutedEventArgs e)
        {
            string selectedDirectory;

            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            using (CommonOpenFileDialog dialog = new()
            {
                Multiselect = false,
                InitialDirectory = PathToSubnautica,
                EnsurePathExists = true,
                IsFolderPicker = true,
                Title = "Select Subnautica installation directory"
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                selectedDirectory = Path.GetFullPath(dialog.FileName);
            }

            if (!GameInstallationFinder.IsSubnauticaDirectory(selectedDirectory))
            {
                LauncherNotifier.Error("Invalid subnautica directory");
                return;
            }

            if (selectedDirectory != PathToSubnautica)
            {
                await LauncherLogic.Instance.SetTargetedSubnauticaPath(selectedDirectory);
                LauncherNotifier.Success("Applied changes");
            }
        }

        private void OnChangeArguments_Click(object sender, RoutedEventArgs e)
        {
            if (ArgumentsTextbox.Text == SubnauticaLaunchArguments)
            {
                return;
            }

            ResetButton.Visibility = SubnauticaLaunchArguments == LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS ? Visibility.Visible : Visibility.Hidden;
            ArgumentsTextbox.Text = LauncherLogic.Config.SubnauticaLaunchArguments = ArgumentsTextbox.Text.Trim();
            LauncherNotifier.Success("Applied changes");
        }

        private void OnResetArguments_Click(object sender, RoutedEventArgs e)
        {
            if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
            {
                ArgumentsTextbox.Text = LauncherLogic.Config.SubnauticaLaunchArguments = LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS;
                ResetButton.Visibility = Visibility.Hidden;
                LauncherNotifier.Success("Applied changes");
                return;
            }
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(PathToSubnautica));
            OnPropertyChanged(nameof(GamePlatform));
        }

        private void OnViewFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WorldManager.SavesFolderDir)?.Dispose();
        }
    }
}

using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxLauncher.Models;
using NitroxModel.Discovery;
using NitroxLauncher.Properties;

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

            LaunchArguments.Text = SubnauticaLaunchArguments;

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

        private async void ChangePath_Click(object sender, RoutedEventArgs e)
        {
            string selectedDirectory;

            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new()
            {
                Multiselect = false,
                InitialDirectory = PathToSubnautica,
                EnsurePathExists = true,
                IsFolderPicker = true,
                Title = "Select Subnautica installation directory"
            };
            using (dialog)
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                selectedDirectory = Path.GetFullPath(dialog.FileName);
            }

            if (GameInstallationFinder.IsSubnauticaDirectory(selectedDirectory))
            {
                await LauncherLogic.Instance.SetTargetedSubnauticaPath(selectedDirectory);
                LauncherNotifier.Success("Applied changes");
            }
            else
            {
                LauncherNotifier.Error("Invalid subnautica directory");
            }
        }

        private void ChangeArguments_Click(object sender, RoutedEventArgs e)
        {
            string newArguments = LaunchArguments.Text;

            if (!string.IsNullOrWhiteSpace(newArguments))
            {
                newArguments = newArguments.Trim();
                LauncherLogic.Config.SubnauticaLaunchArguments = newArguments;
                LaunchArguments.Text = newArguments;
                LauncherNotifier.Success("Applied changes");
            }
            else
            {
                LauncherNotifier.Error("Invalid launch arguments");
                LaunchArguments.Text = SubnauticaLaunchArguments;
            }
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(PathToSubnautica));
            OnPropertyChanged(nameof(GamePlatform));
        }
    }
}

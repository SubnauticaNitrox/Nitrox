using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxLauncher.Models;
using NitroxModel.Discovery;

namespace NitroxLauncher.Pages
{
    public partial class OptionPage : PageBase
    {
        public string PathToSubnautica => LauncherLogic.Config.SubnauticaPath;

        public OptionPage()
        {
            InitializeComponent();
        }

        private async void ChangeOptions_Click(object sender, RoutedEventArgs e)
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
            }
            else
            {
                MessageBox.Show("The selected directory does not contain the required Subnautica.exe file.", "Invalid Subnautica directory", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OptionPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
            OnPropertyChanged(nameof(PathToSubnautica));
        }

        private void OptionPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // Pass-through property change events.
            if (args.PropertyName == nameof(LauncherLogic.Config.SubnauticaPath))
            {
                OnPropertyChanged(nameof(PathToSubnautica));
            }
        }
    }
}

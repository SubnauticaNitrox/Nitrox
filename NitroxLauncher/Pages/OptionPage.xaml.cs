using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace NitroxLauncher.Pages
{
    public partial class OptionPage : PageBase, INotifyPropertyChanged
    {
        public string PathToSubnautica => LauncherLogic.Instance.SubnauticaPath;

        public OptionPage()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void ChangeOptions_Click(object sender, RoutedEventArgs e)
        {
            string selectedDirectory;

            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
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

            if (LauncherLogic.Instance.IsSubnauticaDirectory(selectedDirectory))
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
            LauncherLogic.Instance.PropertyChanged += OnLogicPropertyChanged;
            OnPropertyChanged(nameof(PathToSubnautica));
        }

        private void OptionPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LauncherLogic.Instance.PropertyChanged -= OnLogicPropertyChanged;
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // Pass-through property change events.
            if (args.PropertyName == nameof(LauncherLogic.Instance.SubnauticaPath))
            {
                OnPropertyChanged(nameof(PathToSubnautica));
            }
        }
    }
}

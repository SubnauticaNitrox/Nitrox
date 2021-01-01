using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Diagnostics;

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

        private void ZeroTierInstall(object sender, RoutedEventArgs e)
        {
            // check if zerotier is installed
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ZeroTier", "One", "ZeroTier One.exe")))
            {
                MessageBox.Show("Error: ZeroTier is already installed!", "ZeroTier Installer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // install zerotier
            DownloadFile("https://download.zerotier.com/RELEASES/1.6.2/dist/ZeroTier%20One.msi", Path.Combine(GetDownloadFolderPath(), "ZeroTier One.msi"));
            Process RunInstaller = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(GetDownloadFolderPath(), "ZeroTier One.msi")
                }
            };
            RunInstaller.Start();
            RunInstaller.WaitForExit();
        }
        public static void DownloadFile(string remoteFilename, string localFilename)
        {
            WebClient client = new WebClient();
            client.DownloadFile(remoteFilename, localFilename);
        }
        string GetDownloadFolderPath()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
        }
    }
}

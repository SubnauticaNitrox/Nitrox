using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using NitroxModel.Discovery;

namespace NitroxLauncher
{
    public partial class OptionPage : Page, INotifyPropertyChanged
    {
        private string pathToSubnautica;

        public string PathToSubnautica
        {
            get => pathToSubnautica;
            set
            {
                value = Path.GetFullPath(value); // Ensures the path looks alright (no mixed / and \ path separators)

                pathToSubnautica = value;
                OnPropertyChanged();
                File.WriteAllText("path.txt", value);
            }
        }

        private string lang;
        public string Lang
        {
            get => lang;
            set
            {
                lang = value;
                OnPropertyChanged();
            }
        }

        public OptionPage()
        {
            InitializeComponent();
            PathToSubnautica = GameInstallationFinder.Instance.FindGame(new List<string>()).OrElse(@"C:\Program Files\Epic Games\Subnautica");
            Lang = Localization.GetCurrentCultureLanguage().ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ChangeOptions_Click(object sender, RoutedEventArgs e)
        {
            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = PathToSubnautica,
                EnsurePathExists = true,
                IsFolderPicker = true,
                Title = "Select Subnautica installation directory"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            string selectedDirectory = Path.GetFullPath(dialog.FileName);
            if (IsSubnauticaDirectory(selectedDirectory))
            {
                PathToSubnautica = selectedDirectory;
            }
            else
            {
                MessageBox.Show("The selected directory does not contain the Subnautica.exe file. Please select the Subnautica installation directory.", "No Subnautica found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool IsSubnauticaDirectory(string directory)
        {
            return Directory.EnumerateFileSystemEntries(directory, "*.exe")
                .Any(file => Path.GetFileName(file)?.Equals("subnautica.exe", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}

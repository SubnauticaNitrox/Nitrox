using System.ComponentModel;
using System.Runtime.CompilerServices;
using NitroxLauncher.Properties;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    internal sealed class LauncherConfig : INotifyPropertyChanged
    {
        // Is the Nitrox version the latest available
        private bool isUpToDate = true;
        public bool IsUpToDate
        {
            get => isUpToDate;
            set
            {
                isUpToDate = value;
                OnPropertyChanged();
            }
        }

        // Subnautica game files path
        public string SubnauticaPath
        {
            get => NitroxUser.GamePath;
            set
            {
                // Ensures the path looks alright (no mixed / and \ path separators)
                NitroxUser.GamePath = value;
                OnPropertyChanged();
            }
        }

        public const string DEFAULT_LAUNCH_ARGUMENTS = "-vrmode none";
        // Launch arguments used to launch Subnautica
        private string subnauticaLaunchArguments = Settings.Default.LaunchArgs ?? DEFAULT_LAUNCH_ARGUMENTS;
        public string SubnauticaLaunchArguments
        {
            get => subnauticaLaunchArguments;
            set
            {
                if (value != subnauticaLaunchArguments)
                {
                    subnauticaLaunchArguments = value;
                    Settings.Default.LaunchArgs = value;
                    Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        // Is server external by default
        private bool isExternalServer = Settings.Default.IsExternalServer;
        public bool IsExternalServer
        {
            get => isExternalServer;
            set
            {
                if (value != isExternalServer)
                {
                    isExternalServer = value;
                    Settings.Default.IsExternalServer = value;
                    Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

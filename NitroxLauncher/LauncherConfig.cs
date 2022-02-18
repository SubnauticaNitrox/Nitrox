using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using NitroxLauncher.Properties;
using NitroxModel.Discovery;
using NitroxModel.Platforms.Store;

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

        // Is subnautica a pirated version
        private bool isPirated = false;
        public bool IsPirated
        {
            get => isPirated;
            set
            {
                isPirated = value;
                OnPropertyChanged();
            }
        }

        // Subnautica game platform (Epic, Steam, MS, ...)
        private Platform subnauticaPlatform = Platform.NONE;
        public Platform SubnauticaPlatform
        {
            get => subnauticaPlatform;
            set
            {
                subnauticaPlatform = value;
                OnPropertyChanged();
            }
        }

        // Subnautica game files path
        private string subnauticaPath = string.Empty;
        public string SubnauticaPath
        {
            get => subnauticaPath;
            set
            {
                // Ensures the path looks alright (no mixed / and \ path separators)
                subnauticaPath = Path.GetFullPath(value);
                subnauticaPlatform = GamePlatforms.GetPlatformByGameDir(subnauticaPath)?.platform ?? Platform.NONE;
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

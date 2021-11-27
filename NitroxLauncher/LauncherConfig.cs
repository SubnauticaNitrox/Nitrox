using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using NitroxModel.Discovery;

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
                SubnauticaPlatform = PlatformDetection.GetPlatform(subnauticaPath);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

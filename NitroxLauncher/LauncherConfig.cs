using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using NitroxModel.Discovery;

namespace NitroxLauncher
{
    internal sealed class LauncherConfig : INotifyPropertyChanged
    {
        private Platform subnauticaPlatform;
        public Platform SubnauticaPlatform
        {
            get => subnauticaPlatform;
            set
            {
                subnauticaPlatform = value;
                OnPropertyChanged();
            }
        }

        private string subnauticaPath;
        public string SubnauticaPath
        {
            get => subnauticaPath;
            set
            {
                subnauticaPath = Path.GetFullPath(value); // Ensures the path looks alright (no mixed / and \ path separators)
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

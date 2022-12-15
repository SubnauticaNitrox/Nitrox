using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxLauncher.Models
{
    public abstract class PageBase : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Opens default browser with a specific link
        /// </summary>
        protected void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            FileSystem.Instance.Open(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

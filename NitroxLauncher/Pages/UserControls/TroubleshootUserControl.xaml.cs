using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Pages.UserControls
{
    public partial class TroubleshootUserControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ModuleDependencyProperty = DependencyProperty.Register("ModuleProperty", typeof(TroubleshootModule), typeof(TroubleshootUserControl));
        
        private BitmapImage moduleImage;
        public BitmapImage ModuleImage
        {
            get => moduleImage;
            
            set
            {
                moduleImage = value;
                OnPropertyChanged();
            }
        }

        [Bindable(true)]
        public TroubleshootModule Module
        {
            get => (TroubleshootModule)GetValue(ModuleDependencyProperty);
            set
            {
                SetValue(ModuleDependencyProperty, value);
                OnPropertyChanged();
            }
        }

        public TroubleshootUserControl()
        {
            InitializeComponent();
        }

        #region ------ PropertyChanged ------
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NitroxLauncher.Models.Troubleshoot;

namespace NitroxLauncher.Pages.UserControls
{
    public partial class TroubleshootUserControl : UserControl
    {
        public static readonly DependencyProperty ModuleDependencyProperty = DependencyProperty.Register("ModuleProperty", typeof(TroubleshootModule), typeof(TroubleshootUserControl));

        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapImage ModuleImage { get; set; }

        [Bindable(true)]
        public TroubleshootModule Module
        {
            get
            {
                return (TroubleshootModule)GetValue(ModuleDependencyProperty);
            }

            set
            {
                SetValue(ModuleDependencyProperty, value);
            }
        }

        public TroubleshootUserControl()
        {
            InitializeComponent();
        }
    }
}

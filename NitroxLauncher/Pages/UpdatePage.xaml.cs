using System.Windows.Controls;

namespace NitroxLauncher.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        public string Version => LauncherLogic.Version;

        public UpdatePage()
        {
            InitializeComponent();
        }
    }
}

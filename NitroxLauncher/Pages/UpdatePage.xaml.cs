using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class UpdatePage : PageBase
    {
        public string Version => LauncherLogic.Version;

        public UpdatePage()
        {
            InitializeComponent();
        }
    }
}

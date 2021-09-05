using System.Diagnostics;
using System.Windows.Navigation;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class CommunityPage : PageBase
    {
        public CommunityPage()
        {
            InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            using (Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)))
            {
                e.Handled = true;
            }
        }
    }
}

using NitroxLauncher.Models;
using NitroxLauncher.Models.Troubleshoot;

namespace NitroxLauncher.Pages
{
    public partial class TroubleshootPage : PageBase
    {
        public TroubleshootModule Firewall = new("Firewall");

        public TroubleshootPage()
        {
            InitializeComponent();

            UC_FirewallModule.Module = Firewall;
        }
    }
}

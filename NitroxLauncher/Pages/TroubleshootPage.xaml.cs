using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NitroxLauncher.Models;
using NitroxLauncher.Models.Troubleshoot;
using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Pages
{
    public partial class TroubleshootPage : PageBase
    {
        public static readonly TroubleshootModule Firewall = new FirewallModule();
        public static readonly TroubleshootModule Network = new NetworkModule();
        public static readonly TroubleshootModule Permissions = new PermissionsModule();
        public static readonly TroubleshootModule Antivirus = new AntivirusModule();

        public TroubleshootPage()
        {
            InitializeComponent();
        }

        private void DiagnosticButton_Click(object sender, RoutedEventArgs e)
        {
            Firewall.Status = TroubleshootStatus.RUNNING;

            Task.Delay(3000).ContinueWith((t) =>
            {
                Firewall.Status = TroubleshootStatus.OK;
                Network.Status = TroubleshootStatus.RUNNING;

                Task.Delay(2000).ContinueWith((t) =>
                {
                    Network.Status = TroubleshootStatus.OK;
                    Permissions.Status = TroubleshootStatus.RUNNING;

                    Task.Delay(2000).ContinueWith((t) =>
                    {
                        Permissions.Status = TroubleshootStatus.KO;
                        Antivirus.Status = TroubleshootStatus.RUNNING;

                        Dispatcher.Invoke(() =>
                        {
                            Run_Modules.Text = "Permissions";
                            Run_StateVerb.Text = "needs your";
                            Run_State.Text = "attention";
                            Run_State.Foreground = Brushes.IndianRed;
                        });
                        
                        Task.Delay(1000).ContinueWith((t) =>
                        {
                            Antivirus.Status = TroubleshootStatus.FATAL_ERROR;
                        });
                    });
                });

            });
        }
    }
}

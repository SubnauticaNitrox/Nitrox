using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using NitroxLauncher.Models;
using NitroxLauncher.Models.Troubleshoot;
using NitroxLauncher.Models.Troubleshoot.Abstract;
using NitroxLauncher.Models.Troubleshoot.Abstract.Events;

namespace NitroxLauncher.Pages
{
    public partial class TroubleshootPage : PageBase
    {
        private CancellationTokenSource cancelSource = new();

        public static readonly TroubleshootModule Firewall = new FirewallModule();
        public static readonly TroubleshootModule Network = new NetworkModule();
        public static readonly TroubleshootModule Permissions = new PermissionsModule();
        public static readonly TroubleshootModule Antivirus = new AntivirusModule();

        private static readonly TroubleshootModule[] modules = new TroubleshootModule[]
        {
            Firewall,
            Network,
            Permissions,
            Antivirus
        };

        public TroubleshootPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                TroubleshootModule.LogReceivedEvent += OnLogReceived;
            };
            
            Unloaded += (s, e) =>
            {
                TroubleshootModule.LogReceivedEvent -= OnLogReceived;
            };
        }

        public void OnLogReceived(object sender, LogSentEventArgs e)
        {
            LogWindow.Text += $"[{e.ModuleName}] {e.Message}{Environment.NewLine}";
        }

        private void DiagnosticButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (!button.IsEnabled)
            {
                return;
            }

            button.Visibility = Visibility.Hidden;
            button.IsEnabled = false;

            Dispatcher.InvokeAsync(() =>
            {
                Button_Cancel.Visibility = Visibility.Visible;
                Button_Cancel.IsEnabled = true;

                try
                {
                    foreach (TroubleshootModule module in modules)
                    {
                        module.RunDiagnostic();
                    }

                    Button_Cancel.Visibility = Visibility.Hidden;
                    Button_Cancel.IsEnabled = false;

                    Button_Start.Visibility = Visibility.Visible;
                    Button_Start.IsEnabled = true;
                }
                catch (OperationCanceledException)
                {
                    LauncherNotifier.Error("Unable to launch diagnostic");
                }
            }, DispatcherPriority.Normal, cancelSource.Token);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (!button.IsEnabled)
            {
                return;
            }

            button.Visibility = Visibility.Hidden;
            button.IsEnabled = false;

            foreach (TroubleshootModule module in modules)
            {
                module.Reset();
            }

            Run_StateVerb.Text = "";
            Run_State.Text = "";
            Run_Modules.Text = "Cancel";

            cancelSource.Cancel();
            cancelSource = new CancellationTokenSource();

            Button_Start.Visibility = Visibility.Visible;
            Button_Start.IsEnabled = true;
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NitroxLauncher.Models.Events;
using NitroxLauncher.Models.Properties;
using NitroxLauncher.Pages;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string Version => $"{LauncherLogic.ReleasePhase} {LauncherLogic.Version}";

        public object FrameContent
        {
            get => frameContent;
            set
            {
                frameContent = value;
                OnPropertyChanged();
            }
        }

        public bool UpdateAvailable => !LauncherLogic.Config.IsUpToDate;

        private readonly LauncherLogic logic;
        private bool isServerEmbedded;
        private object frameContent;

        private Button LastButton { get; set; }

        public MainWindow()
        {
            Log.Setup();
            LauncherNotifier.Setup();
            
            logic = new LauncherLogic();

            MaxHeight = SystemParameters.VirtualScreenHeight;
            MaxWidth = SystemParameters.VirtualScreenWidth;

            LauncherLogic.Config.PropertyChanged += (s, e) => OnPropertyChanged(nameof(UpdateAvailable));
            LauncherLogic.Server.ServerStarted += ServerStarted;
            LauncherLogic.Server.ServerExited += ServerExited;

            InitializeComponent();

            // Pirate trigger should happen after UI is loaded.
            Loaded += (sender, args) =>
            {
                // Error if running from a temporary directory because Nitrox Launcher won't be able to write files directly to zip/rar
                // Tools like WinRAR do this to support running EXE files while it's still zipped.
                if (Directory.GetCurrentDirectory().StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Nitrox launcher should not be executed from a temporary directory. Install Nitrox launcher properly by extracting ALL files and moving these to a dedicated location on your PC.",
                                    "Invalid working directory",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    Environment.Exit(1);
                }
                
                // This pirate detection subscriber is immediately invoked if pirate has been detected right now.
                PirateDetection.PirateDetected += (o, eventArgs) =>
                {
                    LauncherLogic.Config.SubnauticaPlatform = Platform.PIRATED;
                    LauncherLogic.Config.IsPirated = true;

                    LauncherNotifier.Info("Nitrox does not support pirated version of Subnautica");
                    LauncherNotifier.Info("Yo ho ho, Ahoy matey! Ye be a pirate!");

                    WebBrowser webBrowser = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0),
                        
                        Height = MinHeight * 0.7,
                        Width = MinWidth * 0.7
                    };

                    FrameContent = webBrowser;

                    string embed = "<html><head>" +
                                   "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
                                   "</head><body>" +
                                   $"<iframe width=\"{webBrowser.Width - 24}\" height=\"{webBrowser.Height - 24}\" src=\"{{0}}\"" +
                                   "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
                                   "</body></html>";
                    webBrowser.NavigateToString(string.Format(embed, "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1&loop=1&showinfo=0&controls=0"));
                    SideBarPanel.Visibility = Visibility.Hidden;
                };

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    Log.Warn("Launcher may not be connected to internet");
                    LauncherNotifier.Error("Launcher may not be connected to internet");
                }

                if (!NitroxEnvironment.IsReleaseMode)
                {
                    LauncherNotifier.Warning("You're now using Nitrox DEV build");
                }
            };

            logic.SetTargetedSubnauticaPath(NitroxUser.SubnauticaPath)
                 .ContinueWith(task =>
                 {
                     if (GameInstallationFinder.IsSubnauticaDirectory(task.Result))
                     {
                         LauncherLogic.Instance.NavigateTo<LaunchGamePage>();
                     }
                     else
                     {
                         LauncherLogic.Instance.NavigateTo<OptionPage>();
                     }

                     logic.CheckNitroxVersion();
                     logic.ConfigureFirewall();
                 }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CanClose()
        {
            if (LauncherLogic.Server.IsServerRunning && isServerEmbedded)
            {
                MessageBoxResult userAnswer = MessageBox.Show("The embedded server is still running. Click yes to close.", "Close aborted", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return userAnswer == MessageBoxResult.Yes;
            }

            return true;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose())
            {
                e.Cancel = true;
                return;
            }

            logic.Dispose();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimze_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            RestoreButton.Visibility = Visibility.Collapsed;
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            isServerEmbedded = e.IsEmbedded;

            if (e.IsEmbedded)
            {
                LauncherLogic.Instance.NavigateTo<ServerConsolePage>();
            }
        }

        private void ServerExited(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                if (LauncherLogic.Instance.NavigationIsOn<ServerConsolePage>())
                {
                    LauncherLogic.Instance.NavigateTo<ServerPage>();
                }
            });
        }

        private void ButtonNavigation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement elem)
            {
                return;
            }
            if (sender is not Button button)
            {
                return;
            }

            LauncherLogic.Instance.NavigateTo(elem.Tag?.GetType());
            if (LastButton == null)
            {
                NavPlayGamePageButton.SetValue(ButtonProperties.SelectedProperty, false);
            }
            else
            {
                LastButton.SetValue(ButtonProperties.SelectedProperty, false);
            }
            LastButton = button;
            button.SetValue(ButtonProperties.SelectedProperty, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

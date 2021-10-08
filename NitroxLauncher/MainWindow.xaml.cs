using System;
using System.ComponentModel;
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
        public string Version => $"{LauncherLogic.RELEASE_PHASE} {LauncherLogic.Version}";

        public object FrameContent
        {
            get => frameContent;
            set
            {
                frameContent = value;
                OnPropertyChanged();
            }
        }

        private readonly LauncherLogic logic = new();
        private bool isServerEmbedded;
        private object frameContent;

        private Button LastButton { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.VirtualScreenHeight;
            MaxWidth = SystemParameters.VirtualScreenWidth;

            // Pirate trigger should happen after UI is loaded.
            Loaded += (sender, args) =>
            {
                // This pirate detection subscriber is immediately invoked if pirate has been detected right now.
                PirateDetection.PirateDetected += (o, eventArgs) =>
                {
                    WebBrowser webBrowser = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(0)
                    };

                    FrameContent = webBrowser;

                    string embed = "<html><head>" +
                                   "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
                                   "</head><body>" +
                                   $"<iframe width=\"{MainFrame.ActualWidth}\" height=\"{MainFrame.ActualHeight}\" src=\"{{0}}\"" +
                                   "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
                                   "</body></html>";
                    webBrowser.NavigateToString(string.Format(embed, "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1"));
                    SideBarPanel.Visibility = Visibility.Hidden;
                };
            };

            LauncherLogic.Server.ServerStarted += ServerStarted;
            LauncherLogic.Server.ServerExited += ServerExited;

            logic.SetTargetedSubnauticaPath(GameInstallationFinder.Instance.FindGame())
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

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreButton.Visibility = Visibility.Visible;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            RestoreButton.Visibility = Visibility.Collapsed;
            MaximizeButton.Visibility = Visibility.Visible;
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

            LauncherLogic.Instance.NavigateTo(elem.Tag?.GetType());

            if (sender is Button button)
            {
                LastButton?.SetValue(ButtonProperties.SelectedProperty, false);
                LastButton = button;
                button.SetValue(ButtonProperties.SelectedProperty, true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

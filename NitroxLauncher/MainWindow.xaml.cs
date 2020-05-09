using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NitroxLauncher.AttachedProperties;
using NitroxLauncher.Events;
using NitroxLauncher.Pages;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly LauncherLogic logic = new LauncherLogic();
        private object frameContent;
        private bool isServerEmbedded;

        public string Version => $"{LauncherLogic.RELEASE_PHASE} {LauncherLogic.Version}";

        public object FrameContent
        {
            get => frameContent;
            set
            {
                frameContent = value;

                // Update navigation buttons styling
                foreach (Button button in SideBarPanel.GetChildrenOfType<Button>())
                {
                    button.SetValue(ButtonProperties.SelectedProperty, button.Tag == value || button.Tag?.GetType() == typeof(ServerPage) && value?.GetType() == typeof(ServerConsolePage));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPageBackground));
            }
        }

        public ImageSource CurrentPageBackground => (ImageSource)(FrameContent as Page)?.Background.GetValue(ImageBrush.ImageSourceProperty);

        public MainWindow()
        {
            InitializeComponent();

            // Pirate trigger should happen after UI is loaded.
            Loaded += (sender, args) =>
            {
                // This pirate detection subscriber is immediately invoked if pirate has been detected right now.
                PirateDetection.PirateDetected += (o, eventArgs) =>
                {
                    WebBrowser webBrowser = new WebBrowser();
                    FrameContent = webBrowser;
                    webBrowser.HorizontalAlignment = HorizontalAlignment.Stretch;
                    webBrowser.VerticalAlignment = VerticalAlignment.Stretch;
                    webBrowser.Margin = new Thickness(0);

                    string embed = "<html><head>" +
                                   "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
                                   "</head><body>" +
                                   $"<iframe width=\"{MainFrame.ActualWidth}\" height=\"{MainFrame.ActualHeight}\" src=\"{{0}}\"" +
                                   "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
                                   "</body></html>";
                    webBrowser.NavigateToString(string.Format(embed, "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1"));
                };
            };

            logic.ServerStarted += ServerStarted;
            logic.ServerExited += ServerExited;

            logic.SetTargetedSubnauticaPath(GameInstallationFinder.Instance.FindGame())
                .ContinueWith(task =>
                    {
                        if (logic.IsSubnauticaDirectory(task.Result))
                        {
                            LauncherLogic.Instance.NavigateTo<LaunchGamePage>();
                        }
                        else
                        {
                            LauncherLogic.Instance.NavigateTo<OptionPage>();
                        }

                        logic.CheckNitroxVersion();
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal async Task CloseInternalServerAndRemovePatchAsync()
        {
            await LauncherLogic.Instance.SendServerCommandAsync("stop\n");
            logic.Dispose();
        }

        private bool CanClose()
        {
            if (logic.ServerRunning && isServerEmbedded)
            {
                MessageBox.Show("The embedded server is still running.", "Close aborted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose())
            {
                e.Cancel = true;
            }
            await CloseInternalServerAndRemovePatchAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (CanClose())
            {
                Close();
            }
        }

        private void Minimze_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            isServerEmbedded = e.Embedded;

            if (e.Embedded)
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
            if (!(sender is FrameworkElement elem))
            {
                return;
            }
            LauncherLogic.Instance.NavigateTo(elem.Tag?.GetType());
        }

        private void PART_VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}

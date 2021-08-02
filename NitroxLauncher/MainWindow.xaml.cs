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
        private readonly LauncherLogic logic = new();
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

            logic.ServerStarted += ServerStarted;
            logic.ServerExited += ServerExited;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanClose()
        {
            if (logic.IsServerRunning && isServerEmbedded)
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
            if (sender is not FrameworkElement elem)
            {
                return;
            }
            LauncherLogic.Instance.NavigateTo(elem.Tag?.GetType());
        }
    }
}

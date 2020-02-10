using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using NitroxLauncher.Commands;
using NitroxLauncher.Events;
using NitroxModel;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.Forms.MessageBox;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace NitroxLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // private readonly Dictionary<Page, BitmapImage> imageDict;
        private readonly LauncherLogic logic = new LauncherLogic();
        private bool isServerEmbedded;
        private RelayCommand<Page> navigationCommand;

        public string Version => "ALPHA " + Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString(3);
        

        public RelayCommand<Page> NavigationCommand => navigationCommand ??= new RelayCommand<Page>(NavigationExecute);

        public MainWindow()
        {
            InitializeComponent();

            // Pirate trigger should happen after UI is loaded.
            Loaded += (sender, args) =>
            {
                // This new pirate detected subscriber is possibly immediately invoked if pirate has been detected right now.
                PirateDetection.PirateDetected += (o, eventArgs) =>
                {
                    string embed = "<html><head>" +
                                   "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
                                   "</head><body>" +
                                   "<iframe width=\"854\" height=\"564\" src=\"{0}\"" +
                                   "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
                                   "</body></html>";
                    Height = 662;
                    Width = 1106;
                    WebBrowser webBrowser = new WebBrowser();
                    MainFrame.Content = webBrowser;
                    webBrowser.HorizontalAlignment = HorizontalAlignment.Stretch;
                    webBrowser.VerticalAlignment = VerticalAlignment.Stretch;
                    webBrowser.Margin = new Thickness(0);
                    webBrowser.NavigateToString(string.Format(embed, "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1"));
                    SideBarPanel.Visibility = BackgroundImage.Visibility = Visibility.Hidden;
                };
            };

            // imageDict = new Dictionary<Page, BitmapImage>
            // {
            //     {
            //         launchPage, new BitmapImage(new Uri(@"/Images/PlayGameImage.png", UriKind.Relative))
            //     },
            //     {
            //         serverPage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative))
            //     },
            //     {
            //         serverConsolePage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative))
            //     },
            //     {
            //         optionPage, new BitmapImage(new Uri(@"/Images/Vines.png", UriKind.Relative))
            //     }
            // };

            logic.ServerStarted += ServerStarted;
            logic.ServerExited += ServerExited;

            logic.SetTargetedSubnauticaPath(GameInstallationFinder.Instance.FindGame().OrElse(null))
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
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        internal async Task CloseInternalServerAndRemovePatchAsync()
        {
            await LauncherLogic.Instance.SendServerCommandAsync("stop\n");
            logic.Dispose();
        }

        private void NavigationExecute(Page page)
        {
            LauncherLogic.Instance.NavigateTo(page.GetType());
        }

        private bool CanClose()
        {
            if (logic.ServerRunning && isServerEmbedded)
            {
                System.Windows.MessageBox.Show("Cannot close as long as the embedded server is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (LauncherLogic.Instance.NavigationIsOn<ServerConsolePage>())
            {
                LauncherLogic.Instance.NavigateTo<ServerPage>();
            }
        }
    }
}

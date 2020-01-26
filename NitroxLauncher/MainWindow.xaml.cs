using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NitroxLauncher.Events;
using NitroxModel;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<Page, BitmapImage> imageDict;
        private readonly LaunchGamePage launchPage;
        private readonly LauncherLogic logic;
        private readonly OptionPage optionPage;
        private readonly ServerConsolePage serverConsolePage;
        private readonly ServerPage serverPage;
        private object currentPage;
        private bool isServerEmbedded;

        public string Version => "ALPHA " + Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString(3);

        public object CurrentPage
        {
            get => currentPage;
            private set
            {
                currentPage = value;
                OnPropertyChanged();
            }
        }

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
                    CurrentPage = webBrowser;
                    webBrowser.HorizontalAlignment = HorizontalAlignment.Stretch;
                    webBrowser.VerticalAlignment = VerticalAlignment.Stretch;
                    webBrowser.Margin = new Thickness(0);
                    webBrowser.NavigateToString(string.Format(embed, "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1"));
                    SideBarPanel.Visibility = BackgroundImage.Visibility = Visibility.Hidden;
                };
            };

            logic = LauncherLogic.Create();
            launchPage = new LaunchGamePage(logic);
            serverPage = new ServerPage(logic);
            optionPage = new OptionPage();
            serverConsolePage = new ServerConsolePage(logic);

            imageDict = new Dictionary<Page, BitmapImage>
            {
                {
                    launchPage, new BitmapImage(new Uri(@"/Images/PlayGameImage.png", UriKind.Relative))
                },
                {
                    serverPage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative))
                },
                {
                    serverConsolePage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative))
                },
                {
                    optionPage, new BitmapImage(new Uri(@"/Images/Vines.png", UriKind.Relative))
                }
            };

            logic.ServerStarted += ServerStarted;
            logic.ServerExited += ServerExited;

            if (!File.Exists("path.txt"))
            {
                ChangeFrameContent(optionPage);
            }
            else
            {
                ChangeFrameContent(launchPage);
            }
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
            await serverConsolePage.SendServerCommandAsync("stop\n");
            logic.Dispose();
        }

        private bool CanClose()
        {
            if (logic.ServerRunning && isServerEmbedded)
            {
                MessageBox.Show("Cannot close as long as the embedded server is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ToPlayGame_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeFrameContent(launchPage);
            SetActive(PlayGameNav);
        }

        private void ToOptions_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeFrameContent(optionPage);
            SetActive(OptionsNav);
        }

        private void ToServer_OnClick(object sender, RoutedEventArgs e)
        {
            if (!logic.ServerRunning || !isServerEmbedded)
            {
                ChangeFrameContent(serverPage);
            }
            else
            {
                ChangeFrameContent(serverConsolePage);
                serverConsolePage.CommandLine.Focus();
            }
            SetActive(ServerNav);
        }

        private void SetActive(Button activeButton)
        {
            foreach (object children in SideBarPanel.Children)
            {
                if (children is Button button)
                {
                    if (button.Content is Grid grid)
                    {
                        foreach (object item in grid.Children)
                        {
                            if (item is TextBlock block)
                            {
                                if (button == activeButton)
                                {
                                    block.FontWeight = FontWeights.Bold;
                                    block.Foreground = Brushes.White;
                                }
                                else // set as not active
                                {
                                    block.FontWeight = FontWeights.Normal;
                                    BrushConverter bc = new BrushConverter();
                                    block.Foreground = (Brush)bc.ConvertFrom("#B2FFFFFF");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            isServerEmbedded = e.Embedded;

            if (e.Embedded)
            {
                ChangeFrameContent(serverConsolePage);
                serverConsolePage.CommandLine.Focus();
            }
            SetActive(ServerNav);
        }

        private void ServerExited(object sender, EventArgs e)
        {
            if (CurrentPage == serverConsolePage)
            {
                CurrentPage = serverPage;
            }
        }

        private void ChangeFrameContent(object frameContent)
        {
            if (frameContent is Page page)
            {
                Height = 542;
                Width = 946;
                CurrentPage = page;
                BackgroundImage.Source = imageDict[page];
                BackgroundImage.Visibility = Visibility.Visible;
            }
        }
    }
}

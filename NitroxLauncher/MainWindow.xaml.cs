using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NitroxModel.Logger;

namespace NitroxLauncher
{    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LauncherLogic logic = new LauncherLogic();
        LaunchGamePage launchPage;
        ServerPage serverPage;
        OptionPage optionPage;
        ServerConsolePage serverConsolePage;
        Dictionary<Page,BitmapImage> imageDict;
        WebBrowser webBrowser = new WebBrowser();
        object currentPage;

        public object CurrentPage
        {
            get { return currentPage; }
            private set
            {
                currentPage = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get
            {                
                return "ALPHA " + Assembly.GetAssembly(typeof(NitroxModel.Extensions)).GetName().Version.ToString(3);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            launchPage = new LaunchGamePage(logic);
            serverPage = new ServerPage(logic);
            optionPage = new OptionPage(logic);
            serverConsolePage = new ServerConsolePage(logic);
            logic.PirateDetectedEvent += PirateDetected;
            logic.StartServerEvent += OnStartServer;
            logic.EndServerEvent += OnEndServer;

            AppDomain.CurrentDomain.UnhandledException += CrashLog;

            imageDict = new Dictionary<Page, BitmapImage> {
                {launchPage, new BitmapImage(new Uri(@"/Images/PlayGameImage.png", UriKind.Relative)) },
                {serverPage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative)) },
                {serverConsolePage, new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative)) },
                {optionPage, new BitmapImage(new Uri(@"/Images/Vines.png", UriKind.Relative)) }
                };

            if (!File.Exists("path.txt"))
            {
                ChangeFrameContent(optionPage);
            }
            else
            {
                ChangeFrameContent(launchPage);
            }
        }

        private void CrashLog(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ToString() + Environment.NewLine + Environment.StackTrace);
        }

        private bool CanClose()
        {
            if (logic.HasSomethingRunning && !serverConsolePage.ServerRunning)
            {
                MessageBox.Show("Cannot close as long as server or game is running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            } else if(serverConsolePage.ServerRunning)
            {
                // If the server is running from launcher, we will just stop the server
                serverConsolePage.HandleInputData("stop\n");
            }
            // If launcher is closing, remove patch from subnautica
            logic.OnSubnauticaExited(this, new EventArgs());
            return true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (CanClose())
            {
                Environment.Exit(0);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose())
            {
                e.Cancel = true;
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
            if (!serverConsolePage.ServerRunning)
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
            foreach (var item in SideBarPanel.Children)
            {
                if(item is Button button)
                {
                    
                    if (button.Content is TextBlock block)
                    {
                        if (button == activeButton)
                        {
                            block.FontWeight = FontWeights.Bold;
                            block.Foreground = Brushes.White;
                        }
                        else // set as not active
                        {
                            block.FontWeight = FontWeights.Normal;
                            var bc = new BrushConverter();
                            block.Foreground = (Brush)bc.ConvertFrom("#B2FFFFFF");
                        }
                    }
                }
            }            
        }

        private void OnStartServer(object sender, EventArgs e)
        {
            ChangeFrameContent(serverConsolePage);
            serverConsolePage.CommandLine.Focus();
            SetActive(ServerNav);
        }

        private void OnEndServer(object sender, EventArgs e)
        {
            if (CurrentPage == serverConsolePage)
            {
                CurrentPage = serverPage;
            }
        }

        private void PirateDetected(object o, EventArgs e)
        {
            string embed = "<html><head>" +
               "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
               "</head><body>" +
               "<iframe width=\"854\" height=\"564\" src=\"{0}\"" +
               "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
               "</body></html>";
            Height = 662;
            Width = 1106;
            BackgroundImage.Visibility = Visibility.Hidden;
            webBrowser.HorizontalAlignment = HorizontalAlignment.Stretch;
            webBrowser.VerticalAlignment = VerticalAlignment.Stretch;
            webBrowser.Margin = new Thickness(0);
            string url = "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1";
            CurrentPage = webBrowser;
            webBrowser.NavigateToString(string.Format(embed, url));
        }

        private void ChangeFrameContent(object frameContent)
        {
            if(frameContent is Page page)
            {
                Height = 542;
                Width = 946;
                CurrentPage = page;
                BackgroundImage.Source = imageDict[page];
                BackgroundImage.Visibility = Visibility.Visible;
            }
            else
            {
                PirateDetected(this, new EventArgs());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        
    }
}

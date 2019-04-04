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

namespace NitroxLauncher
{    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LauncherLogic logic = new LauncherLogic();
        LaunchGamePage launchPage;
        ServerPage serverPage;
        OptionPage optionPage;
        ServerConsolePage serverConsolePage;
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
            if (!File.Exists("path.txt"))
            {
                CurrentPage = optionPage;
            }
            else
            {
                CurrentPage = launchPage;
            }
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
            Height = 542;
            Width = 946;
            CurrentPage = launchPage;
            BackgroundImage.Source = new BitmapImage(new Uri(@"/Images/PlayGameImage.png", UriKind.Relative));
            BackgroundImage.Visibility = Visibility.Visible;
            SetDeActive(OptionsNav);
            SetActive(PlayGameNav);
            SetDeActive(ServerNav);
        }

        private void ToOptions_OnClick(object sender, RoutedEventArgs e)
        {
            Height = 542;
            Width = 946;
            CurrentPage = optionPage;
            BackgroundImage.Source = new BitmapImage(new Uri(@"/Images/Vines.png", UriKind.Relative));
            BackgroundImage.Visibility = Visibility.Visible;
            SetActive(OptionsNav);
            SetDeActive(PlayGameNav);
            SetDeActive(ServerNav);
        }

        private void ToServer_OnClick(object sender, RoutedEventArgs e)
        {
            Height = 542;
            Width = 946;
            if (!serverConsolePage.ServerRunning)
            {
                CurrentPage = serverPage;
            }
            else
            {
                CurrentPage = serverConsolePage;
                serverConsolePage.CommandLine.Focus();
            }
            BackgroundImage.Source = new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative));
            BackgroundImage.Visibility = Visibility.Visible;
            SetActive(ServerNav);
            SetDeActive(PlayGameNav);
            SetDeActive(OptionsNav);
        }

        private void SetDeActive(Button button)
        {
            if (button.Content is TextBlock block)
            {
                block.FontWeight = FontWeights.Normal;
                var bc = new BrushConverter();
                block.Foreground = (Brush)bc.ConvertFrom("#B2FFFFFF");
            }
        }
        private void SetActive(Button button)
        {
            if (button.Content is TextBlock block)
            {
                block.FontWeight = FontWeights.Bold;
                block.Foreground = Brushes.White;
            }
        }

        private void OnStartServer(object sender, EventArgs e)
        {
            Height = 542;
            Width = 946;
            CurrentPage = serverConsolePage;
            serverConsolePage.CommandLine.Focus();
            BackgroundImage.Source = new BitmapImage(new Uri(@"/Images/EscapePod.png", UriKind.Relative));
            BackgroundImage.Visibility = Visibility.Visible;
            SetActive(ServerNav);
            SetDeActive(PlayGameNav);
            SetDeActive(OptionsNav);
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

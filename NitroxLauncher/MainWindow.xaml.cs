using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LauncherLogic logic = new LauncherLogic();
        LaunchGamePage launchPage;
        ServerPage serverPage;
        OptionPage optionPage;
        WebBrowser webBrowser = new WebBrowser();

        public MainWindow()
        {
            InitializeComponent();
            launchPage = new LaunchGamePage(logic);
            serverPage = new ServerPage(logic);
            optionPage = new OptionPage(logic);
            logic.PirateDetectedEvent += PirateDetected;
            if (!File.Exists("path.txt"))
            {
                MainPage.Content = optionPage;
            }
            else
            {
                MainPage.Content = launchPage;
            }
        }        

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if(logic.HasSomethingRunning)
            {
                MessageBox.Show("Cannot close as long as server or game is running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Environment.Exit(0);
        }

        private void Minimze_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ToPlayGame_OnClick(object sender, RoutedEventArgs e)
        {
            Height = 542;
            Width = 946;
            MainPage.Content = launchPage;
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
            MainPage.Content = optionPage;
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
            MainPage.Content = serverPage;            
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
            MainPage.Content = webBrowser;
            webBrowser.NavigateToString(string.Format(embed, url));
        }


    }
}

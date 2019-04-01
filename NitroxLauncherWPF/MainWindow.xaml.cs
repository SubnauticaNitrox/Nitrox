using System;
using System.Collections.Generic;
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

namespace NitroxLauncherWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LaunchGamePage launchPage = new LaunchGamePage();
        OptionPage optionPage = new OptionPage();

        public MainWindow()
        {
            InitializeComponent();
            MainPage.Content = launchPage;
        }        

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Minimze_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ToPlayGame_OnClick(object sender, RoutedEventArgs e)
        {
            MainPage.Content = launchPage;
            SetDeActive(OptionsNav);
            SetActive(PlayGameNav);
        }

        private void ToOptions_OnClick(object sender, RoutedEventArgs e)
        {
            MainPage.Content = optionPage;
            SetActive(OptionsNav);
            SetDeActive(PlayGameNav);
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
    }
}

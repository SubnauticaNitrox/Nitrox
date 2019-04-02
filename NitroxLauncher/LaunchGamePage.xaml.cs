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

namespace NitroxLauncher
{
    /// <summary>
    /// Interaktionslogik für LaunchGamePage1xaml.xaml
    /// </summary>
    public partial class LaunchGamePage : Page
    {
        LauncherLogic logic;
        public LaunchGamePage(LauncherLogic logic)
        {
            InitializeComponent();
            this.logic = logic;
        }

        private void SinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartSingleplayer();
        }

        private void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartMultiplayer();
        }
    }
}

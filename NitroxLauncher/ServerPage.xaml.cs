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
    public partial class ServerPage : Page
    {
        LauncherLogic logic;
        public ServerPage(LauncherLogic logic)
        {
            InitializeComponent();
            this.logic = logic;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            logic.StartServer();
        }
    }
}

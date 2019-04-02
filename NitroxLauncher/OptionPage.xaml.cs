using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
    public partial class OptionPage : Page , INotifyPropertyChanged
    {
        private string pathToSubnautica;
        public string PathToSubnautica {
            get
            {
                return pathToSubnautica;
            }
            set
            {
                pathToSubnautica = value;
                OnPropertyChanged();
                File.WriteAllText("path.txt", value);
            }
        }
        LauncherLogic logic;
        public OptionPage(LauncherLogic logic)
        {
            InitializeComponent();
            this.logic = logic;
            PathToSubnautica = logic.LoadSettings();
        }

        private void ChangeOptions_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                PathToSubnautica = dialog.SelectedPath;
                
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

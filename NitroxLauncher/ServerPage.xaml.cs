﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ServerPage : Page
    {
        LauncherLogic logic;
        bool inLauncher = false;
        public ServerPage(LauncherLogic logic)
        {
            InitializeComponent();

            // Change style depending on windows version. Win 10 uses other definition of comboboxes then win 7 so win 10 has its own style
            if(Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1)
            {
                CBox.Style = (Style)Resources["ComboBoxStyle"];
                CBox.ApplyTemplate();
            }
            this.logic = logic;
        }

        public string Version
        {
            get
            {
                return "NITROX ALPHA " + Assembly.GetAssembly(typeof(NitroxModel.Extensions)).GetName().Version.ToString(3);
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            logic.StartServer(!inLauncher);
        }

        private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex == 0)
            {
                inLauncher = true;
            }
            else
            {
                inLauncher = false;
            }

        }
    }
}

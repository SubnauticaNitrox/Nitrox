using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;

namespace NitroxLauncher
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }
        
        private void Settings_Load(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            Optional<string> installation = GameInstallationFinder.Instance.FindGame(errors);
            string pathToShow = installation.OrElse(@"C:\Program Files\Epic Games\Subnautica");            
            FolderText.Text = pathToShow;
            File.WriteAllText("path.txt", FolderText.Text);            
        }

        private void ChangeFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                FolderText.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            File.WriteAllText("path.txt", FolderText.Text);
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

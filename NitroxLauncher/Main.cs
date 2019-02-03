using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NitroxLauncher.Patching;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;

namespace NitroxLauncher
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SinglePlayerButton_Click(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove();

            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            Process.Start(subnauticaExe, "-EpicPortal");
        }

        private void MultiplayerButton_Click(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Apply();

            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            Process.Start(subnauticaExe, "-EpicPortal");

            //TODO: maybe an async callback to remove when the app closes.
        }

        private void ServerButton_Click(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib", "NitroxServer.exe");
            Process.Start(serverPath);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
            settings.TopMost = true;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if(!File.Exists("path.txt"))
            {
                Settings settings = new Settings();
                settings.Show();
                settings.TopMost = true;
            }
        }

        private bool ErrorConfiguringLaunch(ref string subnauticaPath)
        {
            if (Process.GetProcessesByName("Subnautica").Length > 0)
            {
                MessageBox.Show("An instance of Subnautica is already running", "Error", MessageBoxButtons.OK);
                return true;
            }

            List<string> errors = new List<string>();
            Optional<string> installation = GameInstallationFinder.Instance.FindGame(errors);

            if (!installation.IsPresent())
            {
                MessageBox.Show("Please configure your Subnautica location in settings.", "Error", MessageBoxButtons.OK);
                return true;
            }

            subnauticaPath = installation.Get();

            return false;
        }
        
        private void SyncAssembliesBetweenSubnauticaManagedAndLib(string subnauticaPath)
        {
            string subnauticaManagedPath = Path.Combine(subnauticaPath, "Subnautica_Data", "Managed");
            string libDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib");

            CopyAllAssemblies(subnauticaManagedPath, libDirectory);
            CopyAllAssemblies(libDirectory, subnauticaManagedPath);
        }

        private void CopyAllAssemblies(string source, string destination)
        {
            foreach (string sourceFilePath in Directory.GetFiles(source))
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string destinationFilePath = Path.Combine(destination, fileName);
                
                if (File.Exists(destinationFilePath) && fileName.EndsWith("dll"))
                {
                    Version sourceVersion = AssemblyName.GetAssemblyName(sourceFilePath).Version;
                    Version destinationVersion = AssemblyName.GetAssemblyName(destinationFilePath).Version;

                    if(sourceVersion != destinationVersion)
                    {
                        File.Delete(destinationFilePath);
                        File.Copy(sourceFilePath, destinationFilePath, true);
                    }
                }
                else if(!File.Exists(destinationFilePath))
                {
                    File.Copy(sourceFilePath, destinationFilePath, true);
                }
            }
        }
    }
}

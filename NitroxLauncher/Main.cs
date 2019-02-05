using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NitroxLauncher.Patching;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;

namespace NitroxLauncher
{
    public partial class Main : Form
    {
        private bool isDragging;
        private Point dragStartPoint;

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

            if (PirateDetection.IsPirate(subnauticaPath))
            {
                PirateDetected();
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove();

            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            Process.Start(subnauticaExe);
        }

        private void MultiplayerButton_Click(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            if(PirateDetection.IsPirate(subnauticaPath))
            {
                PirateDetected();
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove(); // Remove any previous instances first.
            nitroxEntryPatch.Apply();

            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            Process.Start(subnauticaExe);

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
            if (!File.Exists("path.txt"))
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

            List<string> ignoreNitroxBinaries = new List<string>() { "NitroxModel.dll", "NitroxPatcher.dll", "NitroxClient.dll", "0Harmony.dll", "Autofac.dll", "log4net.dll", "protobuf-net.dll", "LitJson.dll", "dnlib.dll", "AssetsTools.NET.dll", "LiteNetLib.dll" };
            CopyAllAssemblies(subnauticaManagedPath, libDirectory, ignoreNitroxBinaries);

            List<string> ignoreNoBinaries = new List<string>();
            CopyAllAssemblies(libDirectory, subnauticaManagedPath, ignoreNoBinaries);
        }

        private void CopyAllAssemblies(string source, string destination, List<string> dllsToIgnore)
        {
            foreach (string sourceFilePath in Directory.GetFiles(source))
            {
                string fileName = Path.GetFileName(sourceFilePath);

                if(dllsToIgnore.Contains(fileName))
                {
                    continue;
                } 

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

        private void PirateDetected()
        {
            string embed = "<html><head>" +
               "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\"/>" +
               "</head><body>" +
               "<iframe width=\"854\" height=\"564\" src=\"{0}\"" +
               "frameborder = \"0\" allow = \"autoplay; encrypted-media\" allowfullscreen></iframe>" +
               "</body></html>";
            string url = "https://www.youtube.com/embed/i8ju_10NkGY?autoplay=1";
            webBrowser1.Location = new Point(157, 125);
            webBrowser1.Visible = true;
            webBrowser1.DocumentText = string.Format(embed, url);
        }

        private void DragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            dragStartPoint = e.Location;
        }

        private void DragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        void DragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            { 
                Point p1 = new Point(e.X, e.Y);
                Point p2 = PointToScreen(p1);
                Point p3 = new Point(p2.X - dragStartPoint.X,
                                     p2.Y - dragStartPoint.Y);
                Location = p3;
            }
        }
    }
}

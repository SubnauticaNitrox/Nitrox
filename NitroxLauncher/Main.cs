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
using NitroxModel.Logger;

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

            StartSubnautica(subnauticaPath);
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

            StartSubnautica(subnauticaPath);

            //TODO: maybe an async callback to remove when the app closes.
        }

        private void OnSubnauticaExit(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            Optional<string> installation = GameInstallationFinder.Instance.FindGame(new List<string>());

            if (!installation.IsPresent())
            {
                MessageBox.Show("Please configure your Subnautica location in settings.", "Error", MessageBoxButtons.OK);
                return;
            }

            subnauticaPath = installation.Get();

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove();
            Log.Info("Finished removing patches!");
        }

        private void StartSubnautica(string subnauticaPath)
        {
            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            Process Subnautica;

            if (PlatformDetection.IsEpic(subnauticaPath))
            {
                Subnautica = Process.Start(subnauticaExe, "-EpicPortal");
            }
            else
            {
                string steamPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam\", "SteamExe", @"C:\Program Files (x86)\Steam\Steam.exe");
                Subnautica = new Process();
                Subnautica.StartInfo.FileName = steamPath;
                Subnautica.StartInfo.Arguments = "-applaunch 264710";
                Subnautica.Start();
            }

            Subnautica.EnableRaisingEvents = true;
            Subnautica.Exited += new EventHandler(OnSubnauticaExit);
        }

        private void ServerButton_Click(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib", "NitroxServer-Subnautica.exe");
            Process server = Process.Start(serverPath);
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
                    try
                    {
                        Version sourceVersion = AssemblyName.GetAssemblyName(sourceFilePath).Version;
                        Version destinationVersion = AssemblyName.GetAssemblyName(destinationFilePath).Version;

                        if (sourceVersion != destinationVersion)
                        {
                            File.Delete(destinationFilePath);
                            File.Copy(sourceFilePath, destinationFilePath, true);
                        }
                    }
                    catch (BadImageFormatException)
                    {
                        // note: discord-rpc.dll has no version information and will fail with BadImageFormatException.
                        // This means the discord-rpc.dll is already present in the destination folder and will be ignored.
                        // Only in case of other dll's the error will be logged.
                        if (!fileName.Equals("discord-rpc.dll") && !fileName.Equals("steam_api64.dll"))
                        {
                            Log.Error($"There was an BadImageFormatException determining the version of the assembly: {fileName}");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"There was error during copying the assembly: {fileName}", e);
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

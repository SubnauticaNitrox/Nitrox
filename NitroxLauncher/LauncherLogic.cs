using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NitroxLauncher.Patching;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxLauncher
{
    public class LauncherLogic
    {
        public event EventHandler PirateDetectedEvent;
        public event EventHandler SubnauticaExitedEvent;
        private Process gameProcess = null;
        private Process serverProcess = null;
        public bool HasSomethingRunning {
            get
            {                
                return ((gameProcess != null && !gameProcess.HasExited)  || (serverProcess != null && !serverProcess.HasExited));
            }
        }

        internal void StartSingleplayer()
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            if (PirateDetection.IsPirate(subnauticaPath))
            {
                if(PirateDetectedEvent != null)
                {
                    PirateDetectedEvent(this, new EventArgs());
                }
                return;
            }
            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove();

            StartSubnautica(subnauticaPath);
        }

        internal void StartMultiplayer()
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath))
            {
                return;
            }

            if (PirateDetection.IsPirate(subnauticaPath))
            {
                if (PirateDetectedEvent != null)
                {
                    PirateDetectedEvent(this, new EventArgs());
                }
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove(); // Remove any previous instances first.
            nitroxEntryPatch.Apply();

            StartSubnautica(subnauticaPath);
            if(gameProcess != null)
            {
                gameProcess.Exited += OnSubnauticaExited;
            }
        }

        private void OnSubnauticaExited(object sender, EventArgs e)
        {
            string subnauticaPath = "";

            Optional<string> installation = GameInstallationFinder.Instance.FindGame(new List<string>());

            if (!installation.IsPresent())
            {
                MessageBox.Show("Please configure your Subnautica location in settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            subnauticaPath = installation.Get();

            NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
            nitroxEntryPatch.Remove();
            Log.Info("Finished removing patches!");
        }

        internal void StartSubnautica(string subnauticaPath)
        {
            string subnauticaExe = Path.Combine(subnauticaPath, "Subnautica.exe");
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = subnauticaPath;
            startInfo.FileName = subnauticaExe;

            if (PlatformDetection.IsEpic(subnauticaPath))
            {
                startInfo.Arguments = "-EpicPortal";
            } else
            {
                startInfo.FileName = "steam://run/264710";
            }
            gameProcess = Process.Start(startInfo);            
        }



        internal string LoadSettings()
        {
            List<string> errors = new List<string>();
            Optional<string> installation = GameInstallationFinder.Instance.FindGame(errors);
            if (installation.IsEmpty())
            {
                installation = Optional<string>.Of(installation.OrElse(@"C:\Program Files\Epic Games\Subnautica"));
                File.WriteAllText("path.txt", installation.Get());
            }
            return installation.Get();
        } 

        internal void StartServer()
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath, true))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib", "NitroxServer-Subnautica.exe");
            serverProcess = Process.Start(serverPath);
            
        }


        internal bool ErrorConfiguringLaunch(ref string subnauticaPath, bool serverStart = false)
        {
            if (Process.GetProcessesByName("Subnautica").Length > 0 && !serverStart)
            {
                MessageBox.Show("An instance of Subnautica is already running", "Error",MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            if(serverStart && (serverProcess != null && !serverProcess.HasExited))
            {
                MessageBox.Show("An instance of Nitrox Server is already running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            List<string> errors = new List<string>();
            Optional<string> installation = GameInstallationFinder.Instance.FindGame(errors);

            if (!installation.IsPresent())
            {
                MessageBox.Show("Please configure your Subnautica location in settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (dllsToIgnore.Contains(fileName))
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
                        if (!fileName.Equals("discord-rpc.dll"))
                        {
                            Log.Error($"There was an BadImageFormatException determining the version of the assembly: {fileName}");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"There was error during copying the assembly: {fileName}", e);
                    }
                }
                else if (!File.Exists(destinationFilePath))
                {
                    File.Copy(sourceFilePath, destinationFilePath, true);
                }
            }
        }
    }
}

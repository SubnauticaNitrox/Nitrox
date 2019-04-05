using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        public event EventHandler StartServerEvent;
        public event EventHandler EndServerEvent;
        private Process gameProcess = null;
        private Process serverProcess = null;
        private bool gameStarting = false;
        public bool HasSomethingRunning {
            get
            {                
                return ((gameProcess != null && !gameProcess.HasExited)  || (serverProcess != null && !serverProcess.HasExited) || gameStarting);
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

            try
            {
                SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

                NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
                nitroxEntryPatch.Remove();

                StartSubnautica(subnauticaPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
            
            try
            {
                gameStarting = true;

                SyncAssetBundles(subnauticaPath);
                SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

                NitroxEntryPatch nitroxEntryPatch = new NitroxEntryPatch(subnauticaPath);
                nitroxEntryPatch.Remove(); // Remove any previous instances first.
                nitroxEntryPatch.Apply();

                StartSubnautica(subnauticaPath);
                Thread thread = new Thread(new ThreadStart(AsyncGetProcess));
                thread.Start();
            }
            catch (Exception ex)
            {
                gameStarting = false;
                MessageBox.Show(ex.ToString());
            }            
        }

        private void AsyncGetProcess()
        {
            int waitTimeInMill = 1000;
            int maxStartingTime = 10;
            if (gameProcess == null)
            {                
                for (int i = 0; i < 1000 && (gameProcess == null); i++)
                {
                    // If wait more than ten seconds, mark game as not starting anymore.
                    // This will not stop the thread. Just if someone closes the launcher before, it will work
                    if(maxStartingTime < i*waitTimeInMill)
                    {
                        gameStarting = false;
                    }
                    Process[] processes = Process.GetProcessesByName("Subnautica");
                    if (processes.Count() == 1)
                    {
                        gameProcess = processes[0];
                    }
                    if (gameProcess == null)
                    {
                        Thread.Sleep(waitTimeInMill);
                    }
                }
                if (gameProcess == null)
                {
                    Log.Error("No or multiple subnautica processes found. Cannot remove patches after exited.");
                    gameStarting = false;
                    return;
                }
            }
            gameStarting = false;
            gameProcess.Exited += OnSubnauticaExited;
        }

        internal void OnSubnauticaExited(object sender, EventArgs e)
        {
            gameStarting = false;
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
            } else if (PlatformDetection.IsSteam(subnauticaPath))
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

        internal void StartServer(bool windowed)
        {
            string subnauticaPath = "";

            if (ErrorConfiguringLaunch(ref subnauticaPath, true))
            {
                return;
            }

            SyncAssembliesBetweenSubnauticaManagedAndLib(subnauticaPath);

            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lib", "NitroxServer-Subnautica.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo(serverPath);
            if(!windowed)
            {
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.CreateNoWindow = true;
            }
            serverProcess = Process.Start(startInfo);           
            if (!windowed && StartServerEvent != null)
            {
                StartServerEvent(serverProcess, new EventArgs());
            }
        }

        internal void EndServer()
        {
            if (EndServerEvent != null)
            {
                EndServerEvent(serverProcess, new EventArgs());
            }
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

            List<string> ignoreNitroxBinaries = new List<string>() { "NitroxModel.dll", "NitroxServer.dll", "NitroxServer-Subnautica.dll", "NitroxModel-Subnautica.dll", "NitroxPatcher.dll", "NitroxClient.dll", "0Harmony.dll", "Autofac.dll", "log4net.dll", "protobuf-net.dll", "LitJson.dll", "dnlib.dll", "AssetsTools.NET.dll", "LiteNetLib.dll" };
            CopyAllAssemblies(subnauticaManagedPath, libDirectory, ignoreNitroxBinaries);

            List<string> ignoreNoBinaries = new List<string>();
            CopyAllAssemblies(libDirectory, subnauticaManagedPath, ignoreNoBinaries);
        }

        private void SyncAssetBundles(string subnauticaPath)
        {
            string subnauticaAssetsPath = Path.Combine(subnauticaPath, "AssetBundles");

            string[] assetBundles = Directory.GetFiles("AssetBundles");

            foreach (string assetBundle in assetBundles)
            {
                File.Copy(assetBundle, Path.Combine(subnauticaAssetsPath, Path.GetFileName(assetBundle)), true);
            }
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
